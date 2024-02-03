using static System.Linq.Enumerable;
[assembly: System.Reflection.AssemblyVersion("1.0.0")]

namespace ChroMichiru {
	[Plugin("ChroMichiru")]
	public class Plugin {
		static UnityEngine.Sprite[] lbgs = null;
		static UnityEngine.Sprite[] dbgs = null;
		static UnityEngine.Sprite[] bongos;
		static UIDropdown dropdown = null;

		private static System.Collections.Generic.IEnumerable<UnityEngine.Sprite> LoadImages(string path) {
			try {
				if(!System.IO.Directory.Exists(path))
					System.IO.Directory.CreateDirectory(path);
				return System.Linq.Enumerable.Select(System.IO.Directory.GetFiles(path), (string file) => {
					UnityEngine.Texture2D texture = new UnityEngine.Texture2D(2, 2);
					UnityEngine.ImageConversion.LoadImage(texture, System.IO.File.ReadAllBytes(file));
					UnityEngine.Rect rect = new UnityEngine.Rect(0, 0, texture.width, texture.height);
					return UnityEngine.Sprite.Create(texture, rect, new UnityEngine.Vector2(0, 0), 0.1f);
				});
			} catch(System.Exception ex) {
				UnityEngine.Debug.Log($"[ChroMichiru] {ex.Message}");
				return new UnityEngine.Sprite[0];
			}
		}

		[Init]
		private void Init() {
			try {
				bongos = UnityEngine.AssetBundle.LoadFromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ChroMichiru.angr")).LoadAllAssets<UnityEngine.Sprite>();
				if(bongos.Length == 0) {
					UnityEngine.Debug.Log("[ChroMichiru] Failed to load assets from bundle");
					return;
				}
				string bgpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
				bgpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(bgpath), System.IO.Path.GetFileNameWithoutExtension(bgpath));
				System.Collections.Generic.IEnumerable<UnityEngine.Sprite> bgs = LoadImages(bgpath);
				lbgs = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(bgs, LoadImages(System.IO.Path.Combine(bgpath, "light"))));
				dbgs = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(bgs, LoadImages(System.IO.Path.Combine(bgpath, "dark"))));
				Settings.NotifyBySettingName(nameof(BongoCat), (object obj) => {
					try {
						if(dropdown)
							dropdown.Dropdown.SetValueWithoutNotify(Settings.Instance.BongoCat + 1);
					} catch(System.Exception ex) {
						UnityEngine.Debug.Log($"[ChroMichiru] {ex.Message}");
					}
				});
				UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
				UnityEngine.Debug.Log("[ChroMichiru] Loaded");
			} catch(System.Exception ex) {
				UnityEngine.Debug.Log($"[ChroMichiru] {ex.Message}");
			}
		}
		static System.Collections.Generic.List<string> bongoOptions = new() {"load mapper to view bongo cats"};
		internal static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) {
			UnityEngine.Debug.Log($"OnSceneLoaded({scene.name})");
			try {
				if(scene.name == "01_SongSelectMenu" || scene.name == "02_SongEditMenu") {
					UnityEngine.Sprite[] bgs = Settings.Instance.DarkTheme ? dbgs : lbgs;
					if(bgs.Length > 0) {
						UnityEngine.GameObject bg = UnityEngine.GameObject.Find("BGImageCanvas/Image");
						bg.GetComponent<UnityEngine.UI.Image>().sprite = bgs[UnityEngine.Random.Range(0, bgs.Length)];
					}
				} else if(scene.name == "03_Mapper") {
					UnityEngine.GameObject catObject = UnityEngine.GameObject.Find("Bongo Cat");
					BongoCat bongo = catObject.GetComponent<BongoCat>();
					System.Reflection.FieldInfo bongoCats = typeof(BongoCat).GetField("bongoCats", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

					BongoCatPreset[] presets = (BongoCatPreset[])bongoCats.GetValue(bongo);
					presets = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(presets, new BongoCatPreset[] {
						new BongoCatPreset {
							LeftDownRightUp = bongos[1],
							LeftUpRightUp = bongos[3],
							LeftDownRightDown = bongos[0],
							LeftUpRightDown = bongos[2],
							YOffset = -0.05f,
							Scale = new UnityEngine.Vector2(1.25f, 0.5f),
							name = "Angy",
						}
					}));
					bongoCats.SetValue(bongo, presets);

					bongoOptions = presets.Select(preset => $"Bongo Cat: {preset.name}").Prepend("Bongo Cat: disabled").ToList();
				} else if(scene.name == "04_Options") {
					UnityEngine.GameObject optionsPanel = UnityEngine.GameObject.Find("General Panel/Options Holder/Misc Options");
					dropdown = UnityEngine.Object.Instantiate<UIDropdown>(PersistentUI.Instance.DropdownPrefab, optionsPanel.transform);
					UnityEngine.RectTransform tr = (UnityEngine.RectTransform)dropdown.transform;
					dropdown.transform.localScale = new UnityEngine.Vector3(1f, 1f, 1f);
					tr.sizeDelta = new UnityEngine.Vector2(186f, 30f);
					tr.pivot = new UnityEngine.Vector2(0.5f, 0.5f);
					tr.anchorMax = new UnityEngine.Vector2(0.5f, 1f);
					tr.anchorMin = new UnityEngine.Vector2(0.5f, 1f);
					tr.anchoredPosition = new UnityEngine.Vector2(-10f, -23f);

					dropdown.SetOptions(bongoOptions);
					if(bongoOptions.Count > 1) {
						dropdown.Dropdown.onValueChanged.AddListener((int i) => {
							try {
								Settings.Instance.BongoCat = i - 1;
								Settings.ManuallyNotifySettingUpdatedEvent(nameof(Settings.BongoCat), Settings.Instance.BongoCat);
							} catch(System.Exception ex) {
								UnityEngine.Debug.Log($"[ChroMichiru] {ex.Message}");
							}
						});
						dropdown.Dropdown.SetValueWithoutNotify(Settings.Instance.BongoCat + 1);
					}
				}
			} catch(System.Exception ex) {
				UnityEngine.Debug.Log($"[ChroMichiru] {ex.Message}");
			}
		}

		[Exit]
		private void Exit() {
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
			UnityEngine.Debug.Log("[ChroMichiru] Exiting");
		}
	}
}
