using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Enterwell.Clients.Wpf.Notifications;
using Epoxy;
using KuchiPaku.Models;
using KuchiPaku.ModelsData;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
using NLog;
using KuchiPaku.Resources;

namespace KuchiPaku.ViewModels;

public enum Page
{
	Home,
}


public sealed class LocalizedConsonantOption
{
    public ConsonantOption Option { get; }
    public string Name => Option switch
    {
        ConsonantOption.ALL_N => Res.ConsonantOption_ALL_N,
        ConsonantOption.CONTINUE_BEFORE_VOWEL => Res.ConsonantOption_CONTINUE_BEFORE_VOWEL,
        ConsonantOption.SMALL_MOUSE => Res.ConsonantOption_SMALL_MOUSE,
        _ => Option.ToString(),
    };

    public LocalizedConsonantOption(ConsonantOption option) => Option = option;
}

[ViewModel]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class MainWindowViewModel
{
	private static readonly Logger logger = LogManager.GetCurrentClassLogger();
	public string WindowTitle { get; set; }
	public INotificationMessageManager Manager { get; set; }
	public Command? OpenYmmp { get; set; }
	public string? TargetYmmpFileName { get; set; }

	public ObservableCollection<CharacterListViewModel>? Characters { get; set; }

	public CharacterListViewModel? SelectedCharaItem { get; set; }
	public Command? ItemClick { get; set; }

	public ObservableCollection<LipSyncImageViewModel>? LipSyncImages { get; set; }

	public LocalizedConsonantOption? CurrentConsonantOption { get; set; } =
		new LocalizedConsonantOption(ConsonantOption.CONTINUE_BEFORE_VOWEL);
	public IEnumerable<LocalizedConsonantOption> ConsonantOptionList { get; set; } =
		Enum.GetValues<ConsonantOption>().Select(o => new LocalizedConsonantOption(o));

	public Command? SaveYmmp { get; set; }
	public Command? OpenLicenses { get; set; }

	public Command? OpenWebsite { get; set; }
	public Command? OpenYMM4Website { get; set; }

	public bool IsSaveBackup { get; set; } = true;
	public bool IsOpenWithSave { get; set; } = true;

	public int VisualLeadMs { get; set; } = 66;
	public bool IsEnabledVisualLead { get; set; }

	private JObject? CurrentYmmp { get; set; }

	private string? CurrentYmmpPath { get; set; }

	private int CurrentYmmpFPS { get; set; } = 30;
	IEnumerable<(int Scene, int Fps)> CurrentYmmpSceneFps { get; set; } = [(0,30)];

	public Dictionary<string, LipSyncOption> LipSyncSettings { get; set; } = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string? DebuggerDisplay => ToString();
	static readonly string[] ExtensionTexts = [".png", ".gif", ",webp"];

	public MainWindowViewModel()
	{
		WindowTitle = AppUtil.GetWindowTitle();
		Manager = new NotificationMessageManager();
		Characters = [];
		LipSyncImages = [];
		LipSyncSettings = [];

		KuchiPaku.Core.Models.ConfigUtil.LoadConfig();
		var settings = KuchiPaku.Core.Models.ConfigUtil.Settings;
		foreach (var i in settings!.TalkSoftInterfaces!)
		{
			Debug.WriteLine($"{i.Type}:{i.DllPath}");
		}

		OpenYmmp = Command.Factory.Create<RoutedEventArgs>(async _ =>
		{
			using var cofd = new CommonOpenFileDialog()
			{
				Title = Res.OpenYmmpDialogTitle,
				RestoreDirectory = true,
				IsFolderPicker = false,
			};
			cofd.Filters.Add(new CommonFileDialogFilter(Res.YmmpFileFilter, "*.ymmp"));
			if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
			{
				return;
			}

			Debug.WriteLine($"{cofd.FileName}");
			TargetYmmpFileName = Path.GetFileName(cofd.FileName);

			var json = await YmmpUtil.ReadAsync(cofd.FileName);
			CurrentYmmp = json;
			CurrentYmmpPath = cofd.FileName;
			var fps = YmmpUtil.GetFPS(CurrentYmmp);
			CurrentYmmpSceneFps = fps;

			var ymmChara = await YmmpUtil.ParseCharactersAsync(CurrentYmmp);
			var viewList = ymmChara
				.Where(v => v.TachieCharacterParameter is not null)
				// Enabled PSD tachie support
				// .Where(v => v.TachieType is not YmmpTachieType.PsdTachie)
				.Select(v => new CharacterListViewModel
				{
					Name = v.Name,
					DirectoryPath = v.TachieCharacterParameter.Directory,
					DefaultMouthImgPath = v.TachieDefaultItemParameter.Mouth,
				});

			LipSyncSettings.Clear();
			viewList
				.ToList()
				.ForEach(v =>
				{
					LipSyncSettings.Add(
						v.Name ?? "",
						LipSyncOption.GetDefault(
							v.Name ?? "",
							Path.Combine(v.DirectoryPath ?? "", "口") ?? "",
							v.DefaultMouthImgPath
						)
					);
				});


			Characters.Clear();
			Characters = [.. viewList];

			// Load TOML config if exists
			var configPath = cofd.FileName + ".config.json";
			if (File.Exists(configPath))
			{
				try
				{
					var toml = File.ReadAllText(configPath);
					var config = ProjectConfig.Deserialize(toml);
					foreach (var chara in Characters)
					{
						if (string.IsNullOrEmpty(chara.Name)) continue;
						if (config.Characters.TryGetValue(chara.Name, out var charaConfig))
						{
							chara.IsExport = charaConfig.IsExport;
							if (LipSyncSettings.TryGetValue(chara.Name, out var option))
							{
								option.ConsonantOption = (ConsonantOption)charaConfig.ConsonantOption;
								foreach (var pMap in charaConfig.PhonemeMap)
								{
									option.MousePhonemeImagePair[pMap.Key] = pMap.Value;
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					logger.Error(e, "Failed to load TOML config");
				}
			}

		});

		SaveYmmp = Command.Factory.Create(
			(Func<RoutedEventArgs, ValueTask>)(
				async _ =>
				{
					if (CurrentYmmp is null)
					{
						Manager.Warn(Res.YmmpNotLoadedTitle, Res.YmmpNotLoadedMessage);
						return;
					}

					var sw = new System.Diagnostics.Stopwatch();
					sw.Start();

					var loading = Manager.Loading(Res.SavingTitle, Res.SavingMessage);
					loading.Message = Res.AnalyzingVoiceItemsMessage;
					var ymmp = await YmmpUtil.CopyDeepAsync(CurrentYmmp);
					var voiceItems = await YmmpUtil.ParseVoiceItemsAsync(ymmp);

					sw.Stop();
					Debug.WriteLine($"TIME[ParseVoiceItemsAsync]:{sw.ElapsedMilliseconds}");
					sw.Restart();

					if (voiceItems is null)
					{
						Manager.Dismiss(loading);
						Manager.Info(Res.NoVoiceItemsTitle, Res.NoVoiceItemsMessage);
						return;
					}

					//filter exportable
					loading.Message = Res.FilteringExportItemsMessage;
					var vItems = voiceItems
						.Where(v =>
							Characters.Any(c => c.Name == v.Item.CharacterName)
							&& Characters.First(c => c.Name == v.Item.CharacterName).IsExport
						)
						.ToList()
						;

					var maxLayer = YmmpUtil.GetMaxLayer(ymmp);
					Debug.WriteLine($"MaxLayer: {maxLayer}");

					//カスタムボイス
					//カスタムボイスは同じ場所の.labファイルを探して口パク生成
					var resultCustom = await TryCreateCustomVoiceLipSyncAsync(
						sw,
						loading,
						ymmp,
						vItems,
						maxLayer
					);
					if (!resultCustom)
					{
						return;
					}

					sw.Stop();
					Debug.WriteLine($"TIME[MakeCustomVoiceFaceItem]:{sw.ElapsedMilliseconds}");
					sw.Restart();

					//APIボイス
					var resultApi = await TryCreateApiVoiceLipSyncAsync(
						loading,
						ymmp,
						vItems,
						maxLayer
					);
					if (!resultApi)
					{
						return;
					}

					sw.Stop();
					Debug.WriteLine($"TIME[FilterAPIVoiceAsync]:{sw.ElapsedMilliseconds}");

					//出力
					loading.Message = Res.SavingFileMessage;
					var dir = Directory.Exists(CurrentYmmpPath)
						? Path.GetDirectoryName(CurrentYmmpPath)!
						: AppDomain.CurrentDomain.BaseDirectory;
					using var csfd = new CommonSaveFileDialog()
					{
						Title = Res.SaveYmmpDialogTitle,
						RestoreDirectory = true,
						DefaultDirectory = dir,
						DefaultFileName =
							Path.GetFileNameWithoutExtension(CurrentYmmpPath)
							+ (IsSaveBackup ? ".tmp" : "")
							+ Path.GetExtension(CurrentYmmpPath),
					};
					csfd.Filters.Add(new CommonFileDialogFilter(Res.YmmpFileFilter, "*.ymmp"));

					try
					{
						if (csfd.ShowDialog() != CommonFileDialogResult.Ok)
						{
							Manager.Dismiss(loading);
							return;
						}
					}
					catch (System.Exception e)
					{
						//Manager.Warn(e.Message, e.StackTrace ?? "no stack");
						logger.Info(dir);
						logger.Error(e);
						return;
					}

					sw.Restart();
					await YmmpUtil.SaveAsync(ymmp, csfd.FileName);
					SaveProjectConfig();

					Manager.Dismiss(loading);
					Manager.Info(Res.SaveSuccessTitle, Res.SaveSuccessMessage, true);

					sw.Stop();
					Debug.WriteLine($"TIME[SaveAsync]:{sw.ElapsedMilliseconds}");

					if (IsOpenWithSave)
					{
						await OpenAsync(csfd.FileName);
					}
				}
			)
		);

		//open license folder
		OpenLicenses = Command.Factory.Create<RoutedEventArgs>(async _ =>
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Licenses\");
			await OpenAsync(path);
		});

		OpenWebsite = Command.Factory.Create<RoutedEventArgs>(async _ =>
			await OpenAsync("https://github.com/InuInu2022/KuchiPaku")
		);

		OpenYMM4Website = Command.Factory.Create<RoutedEventArgs>(async _ =>
			await OpenAsync("https://manjubox.net/ymm4/")
		);

		ItemClick = Command.Factory.Create<CharacterListViewModel>(item =>
		{
			if (SelectedCharaItem == item)
			{
				SaveProjectConfig();
				SelectedCharaItem = null;
				LipSyncImages?.Clear();
			}
			else
			{
				SelectedCharaItem = item;
			}
			return default;
		});

	}

	/// <summary>
	/// カスタムボイスの口パク作成
	/// </summary>
	/// <param name="sw"></param>
	/// <param name="loading"></param>
	/// <param name="ymmp"></param>
	/// <param name="voiceItems"></param>
	/// <param name="maxLayer"></param>
	/// <returns></returns>
	private async ValueTask<bool> TryCreateCustomVoiceLipSyncAsync(
		Stopwatch sw,
		INotificationMessage loading,
		JObject ymmp,
		IEnumerable<(int Scene, YmmVoiceItem Item)> voiceItems,
		IDictionary<int, int> maxLayer
	)
	{
		loading.Message = Res.GeneratingCustomVoiceLipSyncMessage;
		var customVoices = await YmmpUtil.FilterCustomVoiceAsync(voiceItems);

		sw.Stop();
		Debug.WriteLine($"TIME[FilterCustomVoiceAsync]:{sw.ElapsedMilliseconds}");
		sw.Restart();

		var name = SelectedCharaItem?.Name ?? "";

		try
		{
			await YmmpUtil.MakeCustomVoiceFaceItemAsync(
				maxLayer,
				[.. customVoices],
				ymmp,
				LipSyncSettings,
				CurrentYmmpSceneFps,
				IsEnabledVisualLead ? VisualLeadMs : 0
			);
		}
		catch (System.Exception e)
		{
			logger.Error(e);
			Manager.Dismiss(loading);
			return false;
		}
		return true;
	}

	/// <summary>
	/// APIボイスの口パク作成
	/// </summary>
	/// <param name="loading"></param>
	/// <param name="ymmp"></param>
	/// <param name="voiceItems"></param>
	/// <param name="maxLayer"></param>
	/// <returns></returns>
	private async ValueTask<bool> TryCreateApiVoiceLipSyncAsync(
		INotificationMessage loading,
		JObject ymmp,
		IEnumerable<(int Scene, YmmVoiceItem Item)> voiceItems,
		IDictionary<int, int> maxLayer
	)
	{
		loading.Message = Res.GeneratingApiVoiceLipSyncMessage;
		var apiVoices = await YmmpUtil.FilterAPIVoiceAsync(voiceItems);

		//APIボイスの口パク生成
		Debug.WriteLine(nameof(YmmpUtil.MakeAPIVoiceFaceItemAsync));
		try
		{
			await YmmpUtil.MakeAPIVoiceFaceItemAsync(
				maxLayer,
				apiVoices,
				ymmp,
				LipSyncSettings,
				CurrentYmmpSceneFps,
				visualLeadMs: IsEnabledVisualLead ? VisualLeadMs : 0
			);
		}
		catch (System.Exception e)
		{
			Debug.WriteLine($"ERROR:{e.Message}");
			logger.Error(e);
			Manager.Dismiss(loading);
			return false;
		}
		return true;
	}


	private void SaveProjectConfig()
	{
		if (string.IsNullOrEmpty(CurrentYmmpPath) || Characters == null) return;

		var configPath = CurrentYmmpPath + ".config.json";
		var config = new ProjectConfig();

		foreach (var chara in Characters)
		{
			if (string.IsNullOrEmpty(chara.Name)) continue;

			var charaConfig = new CharaConfig { IsExport = chara.IsExport };
			if (LipSyncSettings.TryGetValue(chara.Name, out var option))
			{
				charaConfig.ConsonantOption = (int)option.ConsonantOption;
				foreach (var pair in option.MousePhonemeImagePair)
				{
					charaConfig.PhonemeMap[pair.Key] = Path.GetFileName(pair.Value);
				}
			}
			config.Characters[chara.Name] = charaConfig;
		}

		try
		{
			var toml = ProjectConfig.Serialize(config);
			File.WriteAllText(configPath, toml);
		}
		catch (Exception e)
		{
			logger.Error(e, "Failed to save TOML config");
		}
	}

	private static async ValueTask OpenAsync(string path)
	{
		await Task.Run(() =>
		{
			var info = new ProcessStartInfo() { FileName = path, UseShellExecute = true };
			Process.Start(info);
		});
	}


	[PropertyChanged(nameof(SelectedCharaItem))]
	private async ValueTask SelectedCharaItemChangedAsync(CharacterListViewModel item)
	{
		if (item is null)
		{
			return;
		}

		SaveProjectConfig();

		var sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		var chara = item;
		Debug.WriteLine($"SelectedChara: {chara.Name}, isExport: {chara.IsExport}");

		(LipSyncImages ??= []).Clear();

		var path = chara.DirectoryPath!;



		var kuchiDir = await Task.Run(() => Directory.GetDirectories(path, "口").FirstOrDefault());
		var kList = new ObservableCollection<LipSyncImageLineViewModel>();

		if (kuchiDir != null)
		{
			var kuchiImages = Directory
				.GetFiles(kuchiDir)
				.Where(s => ExtensionTexts.Contains(Path.GetExtension(s)))
				.Select(s => new LipSyncImageLineViewModel(Path.GetFileNameWithoutExtension(s), s));
			kList = new ObservableCollection<LipSyncImageLineViewModel>(kuchiImages);
		}

		var images = LipSyncSettings[chara!.Name!]
			.MousePhonemeImagePair.Select(v =>
			{
				var lineName = v.Key switch
				{
					"a" => Res.ALine,
					"i" => Res.ILine,
					"u" => Res.ULine,
					"e" => Res.ELine,
					"o" => Res.OLine,
					"N" => Res.NLine,
					_ => "ERROR",
				};

				int index = 0;
				if (kuchiDir != null)
				{
					var p = Path.Combine(kuchiDir, Path.GetFileName(v.Value));
					var kuchi = kList.FirstOrDefault(k => k.Path == p)
								?? kList.FirstOrDefault(k => k.Path == chara.DefaultMouthImgPath);
					index = (kuchi is null) ? 0 : kList.IndexOf(kuchi);
				}

				return new LipSyncImageViewModel(
					v.Key,
					lineName,
					kList,
					path,
					chara.Name!,
					this,
					index
				);
			})
			.ToList();
		LipSyncImages = [.. images];


		// Sync ConsonantOption UI
		var currentCharaOption = LipSyncSettings[chara.Name!].ConsonantOption;
		CurrentConsonantOption = ConsonantOptionList.FirstOrDefault(o => o.Option == currentCharaOption)
			?? ConsonantOptionList.First();

		sw.Stop();
		Debug.WriteLine($"TIME[rip sync images]:{sw.ElapsedMilliseconds}");
	}


	[PropertyChanged(nameof(CurrentConsonantOption))]
	private async ValueTask CurrentConsonantOptionChangedAsync(LocalizedConsonantOption opt)
	{
		await Application.Current.Dispatcher.InvokeAsync(() =>
		{
			if (SelectedCharaItem != null && !string.IsNullOrEmpty(SelectedCharaItem.Name)) { LipSyncSettings[SelectedCharaItem.Name].ConsonantOption = opt.Option; }
		});
	}
}
