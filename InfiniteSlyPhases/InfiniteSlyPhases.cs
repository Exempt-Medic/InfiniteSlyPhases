using Modding;
using System;
using Satchel.BetterMenus;
using SFCore.Utils;

namespace InfiniteSlyPhases
{
    #region Menu
    public static class ModMenu
    {
        private static Menu? MenuRef;
        public static MenuScreen CreateModMenu(MenuScreen modlistmenu)
        {
            MenuRef ??= new Menu("AbsRad Phase Options", new Element[]
            {
            new CustomSlider(
                "Phase (3 does nothing)",
                f =>
                {
                    InfiniteSlyPhasesMod.LS.infinitePhase = (int)f;
                    InfiniteSlyPhasesMod.LS.instantPhase2 = (int)f != 1 && InfiniteSlyPhasesMod.LS.instantPhase2;
                    MenuRef?.Update();
                },
                () => InfiniteSlyPhasesMod.LS.infinitePhase,
                1f,
                3f,
                true,
                Id:"Phases"),

            BoolOption(
                "Instant Phase 2",
                "Should Phase 2 start immediately?",
                b =>
                {
                    InfiniteSlyPhasesMod.LS.instantPhase2 = InfiniteSlyPhasesMod.LS.infinitePhase != 1 && b;
                    MenuRef?.Update();
                },
                () => InfiniteSlyPhasesMod.LS.instantPhase2,
                Id:"Instant2"
                )
            });

            return MenuRef.GetMenuScreen(modlistmenu);
        }
        public static HorizontalOption BoolOption(
            string name,
            string description,
            Action<bool> applySetting,
            Func<bool> loadSetting,
            string _true = "True",
            string _false = "False",
            string Id = "__UseName")
        {
            if (Id == "__UseName")
            {
                Id = name;
            }

            return new HorizontalOption(
                name,
                description,
                new[] { _true, _false },
                (i) => applySetting(i == 0),
                () => loadSetting() ? 0 : 1,
                Id
            );
        }
    }
    #endregion
    public class InfiniteSlyPhasesMod : Mod, ICustomMenuMod, ILocalSettings<LocalSettings>
    {
        #region Boilerplate
        private static InfiniteSlyPhasesMod? _instance;

        internal static InfiniteSlyPhasesMod Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(InfiniteSlyPhasesMod)} was never constructed");
                }
                return _instance;
            }
        }

        public static LocalSettings LS { get; private set; } = new();
        public void OnLoadLocal(LocalSettings s) => LS = s;
        public LocalSettings OnSaveLocal() => LS;
        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public InfiniteSlyPhasesMod() : base("InfiniteSlyPhases")
        {
            _instance = this;
        }
        #endregion

        #region Init
        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += InfinitePhase1;
            On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter += InfinitePhase2;
            On.HutongGames.PlayMaker.Actions.SetBoolValue.OnEnter += InstantPhase2;

            Log("Initialized");
        }
        #endregion

        private void InstantPhase2(On.HutongGames.PlayMaker.Actions.SetBoolValue.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetBoolValue self)
        {
            if (self.Fsm.GameObject.name == "Sly Boss" && self.Fsm.Name == "Control" && self.State.Name == "Idle" && LS.instantPhase2)
            {
                self.Fsm.FsmComponent.SendEvent("ZERO HP");
            }

            orig(self);
        }

        private void InfinitePhase2(On.HutongGames.PlayMaker.Actions.BoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.BoolTest self)
        {
            orig(self);

            if (self.Fsm.GameObject.name == "Sly Boss" && self.Fsm.Name == "Control" && self.State.Name == "Death Type" && LS.infinitePhase == 2)
            {
                self.Fsm.FsmComponent.RemoveFsmGlobalTransition("ZERO HP");
            }
        }

        private void InfinitePhase1(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Sly Boss" && self.FsmName == "Control" && LS.infinitePhase == 1)
            {
                self.RemoveFsmGlobalTransition("ZERO HP");
            }
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateModMenu(modListMenu);
        public bool ToggleButtonInsideMenu => false;
    }

    public class LocalSettings
    {
        public int infinitePhase = 3;
        public bool instantPhase2 = false;
    }
}
