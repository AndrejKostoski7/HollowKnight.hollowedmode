using Modding;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon.Util;
using System.Collections.Generic;
using UnityEngine;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
//using GlobalEnums;
/*using ModCommon;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;*/
//using System.Net.Mime;
//using System.Runtime.Remoting.Messaging;
//using On;
public class SaveModSettings : ModSettings
{
    public bool beenToCrossroads;
    public bool defeatedMLCrossroads1;
    public bool CrossroadsLever1;
    public bool CrossroadsLever2;
    public bool CrossroadsLever3;
    public int AncientEssence = 0;
    public int MadmanKnowledge = 0;
    public int TraitorLordsKilled = 0;
}
namespace Hollowed_Mode
{
    public class Hollowed_Mode : Mod
    {
        public SaveModSettings Settings = new SaveModSettings();
        public override ModSettings SaveSettings
        {
            get => Settings;
            set => Settings = (SaveModSettings) value;
        }
        private bool isBossfight = false,
            damageMultiplierOn = false,
            loadedOnce = false,
            runEveryFrameScript = false,
            platedShellEqipped = false,
            timedSwitch = false;

        private int counter1 = 0,
            counter2 = 0,
            frameCounter1 = 0, frameCounter2 = 0,
            damageMultiplierAdd = 0, damageMultiplierMultiply = 1,
            champShellDmgMultiply = 0,
            bossNumber = 0,
            frameScriptNumber = 0,
            tabletNumber = 0;

        private GameObject tempGameObject1;

        private static Dictionary<string, GameObject> preloadedGO = new Dictionary<string, GameObject>();
        public Hollowed_Mode() : base("Hollowed Mode")
        {
            
        }
        public override string GetVersion() => "No Eyes";
        
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Tutorial_01", "_Props/Cave Spikes"),
                ("Tutorial_01", "_Props/Cave Spikes (2)"),
                ("Fungus1_30", "fungd_spikes_06"),
                ("Fungus1_30", "fungd_spikes_05"),
                ("Fungus1_30", "Thorn Collider (7)"),
                ("Fungus2_13", "Direction Pole Bench"),
                ("Tutorial_01", "_Scenery/plat_float_19"),
                ("Crossroads_07", "_Scenery/plat_lift_11"),
                //("GG_Ghost_Xero", "Warrior/Ghost Warrior Xero/White Flash"),
                //("GG_Ghost_No_Eyes", "Warrior/Ghost Warrior No Eyes/Warp Out"),
                ("Town", "RestBench"),
                ("Crossroads_13", "_Enemies/Worm"),
                ("Tutorial_01", "_Props/Tut_tablet_top (2)"),
                ("Tutorial_01", "_Enemies/Buzzer 1"),
                ("Crossroads_04", "_Enemies/Fly Spawn/Fly 1"),
                //("White_Palace_05", "wp_saw (14)"),
                ("Deepnest_44", "shadow_gate"),
                ("GG_Hollow_Knight", "Battle Scene/Focus Blasts/HK Prime Blast (4)/Blast"),
                ("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 25/Electric Mage New"),
                ("Crossroads_03", "_Props/Toll Gate"),
                ("Crossroads_03", "_Props/Toll Gate Switch"),
                ("Crossroads_15", "_Enemies/Zombie Shield 1"),
                ("Crossroads_48","Zombie Guard"),
                ("Crossroads_35","Infected_Parent/Hatcher (1)"),
                ("Fungus1_19", "_Enemies/Fat Fly"),
                ("Crossroads_22", "Spitter (3)"),
                ("Fungus3_24", "Zap Cloud (1)"),
                //("Fungus2_08", "Mushroom Turret"),
                ("Ruins1_17", "Ruins Sentry 1"),
                ("Ruins1_17", "Ruins Flying Sentry Javelin (1)"),
                ("Deepnest_East_04", "Super Spitter"),
                ("GG_Mage_Knight", "Mage Knight"),
                ("GG_Traitor_Lord", "Battle Scene/Wave 3/Mantis Traitor Lord"),
                ("GG_Soul_Master", "Mage Lord")
            };
        }
        
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            base.Initialize();
            ModHooks.Instance.SavegameSaveHook += this.SaveGameAction;
            ModHooks.Instance.CharmUpdateHook += this.ChangeCharms;
            ModHooks.Instance.HitInstanceHook += this.HitInstanceAdjust;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
            On.HealthManager.TakeDamage += this.EnemyHit;
            ModHooks.Instance.TakeHealthHook += OnHealthTaken;
            ModHooks.Instance.LanguageGetHook += TextDisplayInGame;
            ModHooks.Instance.HeroUpdateHook += OnHeroUpdate;
            preloadedGO.Add("SpikeCol", preloadedObjects["Tutorial_01"]["_Props/Cave Spikes"]);
            preloadedGO.Add("Spikes", preloadedObjects["Tutorial_01"]["_Props/Cave Spikes (2)"]);
            preloadedGO.Add("ThornSmall1", preloadedObjects["Fungus1_30"]["fungd_spikes_05"]);
            preloadedGO.Add("ThornSmall2", preloadedObjects["Fungus1_30"]["fungd_spikes_06"]);
            preloadedGO.Add("ThornCol", preloadedObjects["Fungus1_30"]["Thorn Collider (7)"]);
            preloadedGO.Add("BenchDirectionPole", preloadedObjects["Fungus2_13"]["Direction Pole Bench"]);
            preloadedGO.Add("LongPlat", preloadedObjects["Tutorial_01"]["_Scenery/plat_float_19"]);
            preloadedGO.Add("LiftPlat", preloadedObjects["Crossroads_07"]["_Scenery/plat_lift_11"]);
            //preloadedGO.Add("DreamFlash", preloadedObjects["GG_Ghost_Xero"]["Warrior/Ghost Warrior Xero/White Flash"]);
            //preloadedGO.Add("DreamFlash3", preloadedObjects["GG_Ghost_No_Eyes"]["Warrior/Ghost Warrior No Eyes/Warp Out"]);
            preloadedGO.Add("Bench", preloadedObjects["Town"]["RestBench"]);
            preloadedGO.Add("LoreTablet", preloadedObjects["Tutorial_01"]["_Props/Tut_tablet_top (2)"]);
            preloadedGO.Add("GoamWorm", preloadedObjects["Crossroads_13"]["_Enemies/Worm"]);
            //preloadedGO.Add("Saw", preloadedObjects["White_Palace_05"]["wp_saw (14)"]);
            preloadedGO.Add("ShadowGate", preloadedObjects["Deepnest_44"]["shadow_gate"]);
            preloadedGO.Add("Focus Blast", preloadedObjects["GG_Hollow_Knight"]["Battle Scene/Focus Blasts/HK Prime Blast (4)/Blast"]);
            preloadedGO.Add("VoltTwister", preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 25/Electric Mage New"]);
            preloadedGO.Add("Lever", preloadedObjects["Crossroads_03"]["_Props/Toll Gate Switch"]);
            preloadedGO.Add("LevGate", preloadedObjects["Crossroads_03"]["_Props/Toll Gate"]);
            preloadedGO.Add("Vengefly", preloadedObjects["Tutorial_01"]["_Enemies/Buzzer 1"]);
            preloadedGO.Add("Gruz", preloadedObjects["Crossroads_04"]["_Enemies/Fly Spawn/Fly 1"]);
            preloadedGO.Add("Aspid", preloadedObjects["Crossroads_22"]["Spitter (3)"]);
            preloadedGO.Add("Oble", preloadedObjects["Fungus1_19"]["_Enemies/Fat Fly"]);
            preloadedGO.Add("ShieldZombie", preloadedObjects["Crossroads_15"]["_Enemies/Zombie Shield 1"]);
            preloadedGO.Add("ZombieGuard", preloadedObjects["Crossroads_48"]["Zombie Guard"]);
            preloadedGO.Add("AspidMother", preloadedObjects["Crossroads_35"]["Infected_Parent/Hatcher (1)"]);
            preloadedGO.Add("ZapCloud", preloadedObjects["Fungus3_24"]["Zap Cloud (1)"]);
            //preloadedGO.Add("Mushroom Turret", preloadedObjects["Fungus2_08"]["Mushroom Turret"]);
            preloadedGO.Add("SentryNail", preloadedObjects["Ruins1_17"]["Ruins Sentry 1"]);
            preloadedGO.Add("SentrySpear", preloadedObjects["Ruins1_17"]["Ruins Flying Sentry Javelin (1)"]);
            preloadedGO.Add("PrimalAss", preloadedObjects["Deepnest_East_04"]["Super Spitter"]);
            preloadedGO.Add("Mage Knight", preloadedObjects["GG_Mage_Knight"]["Mage Knight"]);
            preloadedGO.Add("Traitor Lord", preloadedObjects["GG_Traitor_Lord"]["Battle Scene/Wave 3/Mantis Traitor Lord"]);
            preloadedGO.Add("Soul Master", preloadedObjects["GG_Soul_Master"]["Mage Lord"]);
        }
        //-----------------------------------------------------------------------------------------------------
        //                                           Adding Game Objects
        //-----------------------------------------------------------------------------------------------------

        void addTablet(float posX, float posY, float posZ, float color1, float color2, float color3)
        {
            var tablet = GameObject.Instantiate(Hollowed_Mode.preloadedGO["LoreTablet"]);
            tablet.transform.position = new Vector3(posX, posY, posZ);
            tablet.SetActive(true);
            var children1 = tablet.GetComponentsInChildren<SpriteRenderer>();
            foreach (var child1 in children1)
            {
                if (child1.name == "lit_tablet")
                {
                    child1.gameObject.GetComponent<SpriteRenderer>().color = new Color(color1, color2, color3);
                }
            }
        }
        void addBench(float posX, float posY, float posZ)
        {
            var bench = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Bench"]);
            bench.transform.position = new Vector3(posX, posY, posZ);
            bench.SetActive(true);
        }
        void addSpike(float posX, float posY, float rotation)
        {
            var spike1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Spikes"]);
            spike1.transform.position = new Vector3(posX, posY, 0f);
            spike1.transform.Rotate(0f, 0f, rotation);
            spike1.SetActive(true);
        }
        void addSpkCollision(float posX, float posY, float scaleX, float scaleY)
        {
            var spikeC1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["SpikeCol"]);
            spikeC1.transform.position = new Vector3(posX, posY, 0f);
            spikeC1.transform.SetScaleX(scaleX);
            spikeC1.transform.SetScaleY(scaleY);
            spikeC1.SetActive(true);
        }
        void addThornSmall(float posX, float posY, string variant, float rotation, float scale)
        {
            var spike1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["ThornSmall"+variant]);
            spike1.transform.position = new Vector3(posX, posY, 0f);
            spike1.transform.localScale = new Vector3(scale, scale, 1f);
            spike1.transform.Rotate(0f, 0f, rotation);
            spike1.SetActive(true);
        }
        void addThornCollision(float posX, float posY, float scaleX, float scaleY, float rotation)
        {
            //Set to rotation 272f for it to be right side up
            var spikeC1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["ThornCol"]);
            spikeC1.transform.position = new Vector3(posX, posY, 0f);
            spikeC1.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            spikeC1.transform.Rotate(0f, 0f, rotation);
            spikeC1.SetActive(true);
        }
        void addDirectionPoleBench(float posX, float posY, float rotationY, float rotationZ)
        {
            var spikeC1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["BenchDirectionPole"]);
            spikeC1.transform.position = new Vector3(posX, posY, 0f);;
            spikeC1.transform.Rotate(0f, rotationY, rotationZ);
            spikeC1.SetActive(true);
        }
        void addShadowGate(float posX, float posY, float scaleX, float scaleY)
        {
            var plat1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["ShadowGate"]);
            plat1.transform.position = new Vector3(posX, posY, 0f);
            plat1.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            plat1.SetActive(true);
        }
        void addPlatHeavy(float posX, float posY, float scaleX, float scaleY)
        {
            var plat1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["LongPlat"]);
            plat1.transform.position = new Vector3(posX, posY, 0f);
            plat1.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            plat1.SetActive(true);
        }
        void addPlatLift(float posX, float posY, float rotation, float scaleX, float scaleY)
        {
            var plat1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["LiftPlat"]);
            plat1.transform.position = new Vector3(posX, posY, -0.1f);
            plat1.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            plat1.transform.Rotate(0f, 0f, rotation);
            plat1.SetActive(true);
        }
        void addLocalSwitch(float posX, float posY, string gateName, float rot, int permaSwitchId, bool activated)
        {
            var lever1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Lever"]);
            var leverfsm = lever1.LocateMyFSM("toll switch");
            if (activated == false)
            {
                leverfsm.RemoveAction("Initiate", 4);
                leverfsm.SetState("Pause");
                leverfsm.GetAction<FindGameObject>("Initiate", 2).objectName = gateName;
                leverfsm.RemoveAction("Initiate", 3);
                leverfsm.InsertMethod("Hit", 0, () => updateGameFlags(permaSwitchId));
            }
            lever1.transform.position = new Vector3(posX, posY, 0f);
            lever1.transform.Rotate(0f, 0f, rot);
            lever1.SetActive(true);
        }
        void addLocalSwitchGate(float posX, float posY, string gateName, float scaleX, float scaleY, bool active)
        {
            var leverG1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["LevGate"]);
            leverG1.transform.position = new Vector3(posX, posY, 0f);
            leverG1.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            leverG1.name = gateName;
            leverG1.LocateMyFSM("Toll Gate").SetState("Idle");
            leverG1.SetActive(active);
        }
        void addGoamWorm(float posX, float posY, float scale, float rotation, float waitTime, bool startDown)
        {
            var zap3 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["GoamWorm"]);
            zap3.transform.position = new Vector3(posX, posY+2.7f, 0f);
            zap3.transform.localScale = new Vector3(scale, scale, 1f);
            zap3.transform.Rotate(0f, 0f, rotation);
            PlayMakerFSM fsm  = zap3.LocateMyFSM("Worm Control");
            fsm.GetAction<Wait>("Down", 2).time = waitTime;
            fsm.GetAction<Wait>("Up", 3).time = waitTime;
            fsm.FsmVariables.FindFsmBool("Start Down").Value = startDown;
            zap3.SetActive(true);
        }
        void addZapCloud(float posX, float posY, float scaleX, float scaleY)
        {
            var zap3 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["ZapCloud"]);
            zap3.transform.position = new Vector3(posX, posY, 0f);
            zap3.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            zap3.SetActive(true);
        }
        void addVengefly(float posX, float posY, int health)
        {
            var veng = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Vengefly"]);
            veng.transform.position = new Vector3(posX, posY, 0f);
            veng.GetComponent<HealthManager>().hp = health;
            veng.SetActive(true);
        }
        void addGruzzer(float posX, float posY, int health)
        {
            var veng = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Gruz"]);
            veng.transform.position = new Vector3(posX, posY, 0f);
            veng.GetComponent<HealthManager>().hp = health;
            veng.SetActive(true);
        }
        void addAspidHunter(float posX, float posY)
        {
            var asp2 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Aspid"]);
            asp2.transform.position = new Vector3(posX, posY, 0f);
            asp2.SetActive(true);
        }
        void addBigAspid(float posX, float posY)
        {
            var asp1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Aspid"]);
            asp1.transform.position = new Vector3(posX, posY, 0f);
            asp1.transform.localScale = new Vector3(1.7f, 1.7f, 1f);
            asp1.SetActive(true);
            PlayMakerFSM fsm  = asp1.LocateMyFSM("spitter");
            fsm.GetAction<DistanceFly>("Fly Back", 1).speedMax = 15f;
            fsm.GetAction<DistanceFly>("Fly Back", 1).acceleration = 0.5f;
            fsm.GetAction<DistanceFly>("Distance Fly", 2).speedMax = 15f;
            fsm.GetAction<DistanceFly>("Distance Fly", 2).acceleration = 0.25f;
            fsm.GetAction<Wait>("Fly Back",0).time = 0.05f;
            fsm.ChangeTransition("Fire Dribble", "WAIT", "Fire Anticipate");
            asp1.GetComponent<HealthManager>().hp = 35;
        }
        void addAspidMother(float posX, float posY, int health, int babyHealth)
        {
            var asp1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["AspidMother"]);
            asp1.transform.position = new Vector3(posX, posY, 0f);
            asp1.SetActive(true);
            PlayMakerFSM fsm  = asp1.LocateMyFSM("Hatcher");
            fsm.ChangeTransition("Distance Fly", "WAIT", "Fire Anticipate");
            fsm.GetAction<IntCompare>("Hatched Max Check", 0).integer1 = 0;
            fsm.GetAction<IntCompare>("Hatched Max Check", 0).integer2 = 999;
            fsm.InsertMethod("Fire", 0,() => addPrimalAspid(asp1.transform.position.x, asp1.transform.position.y - 1f, babyHealth, 0.8f));
            asp1.GetComponent<HealthManager>().hp = health;
        }
        void addZombieGuard(float posX, float posY, int plusHealth)
        {
            var dirtmouthSentry = GameObject.Instantiate(Hollowed_Mode.preloadedGO["ZombieGuard"]);
            dirtmouthSentry.transform.position = new Vector3(posX, posY, 0f);
            dirtmouthSentry.GetComponent<HealthManager>().hp += plusHealth;
            dirtmouthSentry.SetActive(true);
        }
        void addObble(float posX, float posY, int health)
        {
            var oble1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Oble"]);
            oble1.GetComponent<HealthManager>().hp = health;
            oble1.transform.position = new Vector3(posX, posY, 0f);
            oble1.SetActive(true);
        }
        void addMushroomTurret(float posX, float posY, int healthPlus)
        {
            var sentry1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Mushroom Turret"]);
            sentry1.transform.position = new Vector3(posX, posY, 0f);
            sentry1.GetComponent<HealthManager>().hp += healthPlus;
            sentry1.SetActive(true);
        }
        void addSentryNail(float posX, float posY)
        {
            var sentry1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["SentryNail"]);
            sentry1.transform.position = new Vector3(posX, posY, 0f);
            sentry1.SetActive(true);
        }
        void addSentrySpear(float posX, float posY)
        {
            var tutShldZom2 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["SentrySpear"]);
            tutShldZom2.transform.position = new Vector3(posX, posY, 0f);
            tutShldZom2.SetActive(true);
        }
        void addMageKnight(float posX, float posY, int health)
        {
            var boss = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Mage Knight"]);
            boss.transform.position = new Vector3(posX, posY, 0f);
            boss.SetActive(true);
            boss.GetComponent<HealthManager>().hp = health;
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Mage Knight");
            bossFsm.GetAction<WaitRandom>("Idle", 5).timeMin = 0.01f;
            bossFsm.GetAction<WaitRandom>("Idle",5).timeMax = 0.1f;
            bossFsm.InsertMethod("Stomp Recover", 0, () => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 3.4f, 2.5f, 11f, 3));
            bossFsm.InsertMethod("Stomp Recover", 0, () => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 3.4f, 2.5f, 11f, 3));
            bossFsm.InsertMethod("Stomp Recover", 0, () => Log("Boss position is: "+boss.transform.position.x+" "+boss.transform.position.y));
        }
        void addVoltTwister(float posX, float posY, int health)
        {
            var boss = GameObject.Instantiate(Hollowed_Mode.preloadedGO["VoltTwister"]);
            boss.transform.position = new Vector3(posX, posY, 0f);
            boss.SetActive(true);
            boss.GetComponent<HealthManager>().hp = health;
        }
        void addPrimalAspid(float posX, float posY, int healthPlus, float size)
        {
            var asp1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["PrimalAss"]);
            asp1.transform.position = new Vector3(posX, posY, 0f);
            asp1.transform.localScale = new Vector3(size, size, size);
            asp1.GetComponent<HealthManager>().hp = healthPlus;
            asp1.SetActive(true);
            PlayMakerFSM fsm  = asp1.LocateMyFSM("spitter");
            fsm.GetAction<DistanceFly>("Fly Back", 1).speedMax = 15f;
            fsm.GetAction<DistanceFly>("Fly Back", 1).acceleration = 0.5f;
            fsm.GetAction<DistanceFly>("Distance Fly", 2).speedMax = 15f;
            fsm.GetAction<DistanceFly>("Distance Fly", 2).acceleration = 0.25f;
            fsm.GetAction<Wait>("Fly Back",0).time = 0.05f;
            fsm.SetState("Init");
        }
        void addBigPrimalAspid(float posX, float posY, int healthPlus)
        {
            var asp1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["PrimalAss"]);
            asp1.transform.position = new Vector3(posX, posY, 0f);
            asp1.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
            asp1.GetComponent<HealthManager>().hp *= 3;
            asp1.GetComponent<HealthManager>().hp += healthPlus;
            asp1.SetActive(true);
            PlayMakerFSM fsm  = asp1.LocateMyFSM("spitter");
            fsm.GetAction<DistanceFly>("Fly Back", 1).speedMax = 15f;
            fsm.GetAction<DistanceFly>("Fly Back", 1).acceleration = 0.5f;
            fsm.GetAction<DistanceFly>("Distance Fly", 2).speedMax = 15f;
            fsm.GetAction<DistanceFly>("Distance Fly", 2).acceleration = 0.25f;
            fsm.GetAction<Wait>("Fly Back",0).time = 0.05f;
            fsm.ChangeTransition("Fire Dribble", "WAIT", "Fire Anticipate");
            fsm.SetState("Init");
        }
        void addAspidTurret(float posX, float posY, int health, bool isPrimal)
        {
            string entity;
            if (isPrimal) entity = "PrimalAss";
            else entity = "Aspid";
            var asp1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO[entity]);
            asp1.transform.position = new Vector3(posX, posY, 0f);
            asp1.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
            asp1.GetComponent<HealthManager>().hp = health;
            asp1.SetActive(true);
            PlayMakerFSM fsm  = asp1.LocateMyFSM("spitter");
            fsm.GetAction<DistanceFly>("Fly Back", 1).speedMax = 0f;
            fsm.GetAction<DistanceFly>("Fly Back", 1).acceleration = 0f;
            fsm.GetAction<DistanceFly>("Distance Fly", 2).speedMax = 0f;
            fsm.GetAction<DistanceFly>("Distance Fly", 2).acceleration = 0f;
            fsm.GetAction<Wait>("Fly Back",0).time = 0.05f;
            if(isPrimal == false) fsm.ChangeTransition("Fire Dribble", "WAIT", "Fire Anticipate");
            fsm.SetState("Init");
        }
        
        //-----------------------------------------------------------------------------------------------------
        //                                               Traitor Lords
        //-----------------------------------------------------------------------------------------------------
        void addTraitorLord(float posX, float posY, float scale, int damage, int variant)
        {
            var boss = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Traitor Lord"]);
            boss.transform.position = new Vector3(posX, posY, 0f);
            boss.transform.localScale = new Vector3(scale, scale, 1f);
            foreach (DamageHero x in boss.GetComponentsInChildren<DamageHero>(true))
                x.damageDealt = damage;
            PlayMakerFSM zeroHpFsm = boss.LocateMyFSM("Zero HP Detect");
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Mantis");
            bossFsm.GetAction<Wait>("Sick Throw CD", 1).time = 0.01f;
            bossFsm.GetAction<Wait>("Cooldown", 2).time = 0.01f;
            bossFsm.GetAction<Wait>("Idle", 6).time = 0.01f;
            bossFsm.ChangeTransition("Attack Swipe", "FINISHED", "Sickle Antic");
            switch (variant)
            {
                case 1:
                    boss.GetComponent<HealthManager>().hp = 2;
                    bossFsm.GetAction<SpawnObjectFromGlobalPool>("Waves", 0).gameObject = new GameObject();
                    bossFsm.GetAction<SpawnObjectFromGlobalPool>("Waves", 3).gameObject = new GameObject();
                    bossFsm.InsertMethod("Waves", 0, () => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 3.4f, 2.5f, 11f, 3));
                    bossFsm.InsertMethod("Waves", 0, () => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 3.4f, 2.5f, 11f, 3));
                    bossFsm.InsertMethod("Waves", 0, () => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 3.4f, 1.2f, 22f, 3));
                    bossFsm.InsertMethod("Waves", 0, () => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 3.4f, 1.2f, 22f, 3));
                    bossFsm.ChangeTransition("Attack Choice", "DSLASH", "Slam Antic");
                    bossFsm.ChangeTransition("Slam End", "FINISHED", "Attack Swipe");
                    zeroHpFsm.InsertMethod("Set", 0, () => updateGameFlags(3));
                    break;
            } 

            boss.SetActive(true);
        }
        void updateGameFlags(int flagid)
        {
            switch (flagid)
            {
                //Crossroads Enterance Lever
                case 1:
                    Settings.CrossroadsLever1 = true;
                    Log("The Crossroads Enterance Levers have been activated");
                    break;
                //Crossroads Cornifer Lever
                case 2:
                    Settings.CrossroadsLever2 = true;
                    Log("The Cornifer Lever Gate has been activated");
                    break;
                //Crossroads Mantis Lord Defeated
                case 3:
                    Log("The Mantis Lord in the Crossroads has been defeated");
                    Log("This will set a switch in the final game and will award the player with 100 geo");
                    break;
                case 4:
                    Log("Tall room lever has been opened");
                    Settings.CrossroadsLever3 = true;
                    break;
                case 5:
                    Log("Timed switch in crossroads has been activated");
                    addAspidMother(62.7f, 51.5f, 35, 15);
                    addAspidMother(62.7f, 50.5f, 35, 15);
                    addAspidMother(59.5f, 52.0f, 35, 15);
                    addAspidMother(56.4f, 48.5f, 35, 15);
                    addAspidMother(49f, 45f, 35, 15);
                    addBigAspid(60f, 47f);
                    addBigAspid(49f, 47f);
                    addBigAspid(55f, 48f);
                    addBigAspid(35f, 43f);
                    addBigAspid(35f, 40f);
                    timedSwitch = true;
                    break;
            }
        }
        //-----------------------------------------------------------------------------------------------------
        //                                            Boss and AI changes
        //-----------------------------------------------------------------------------------------------------
        /*private void createDreamFlash(float posX, float posY, float scale)
        {
            var asp1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["DreamFlash"]);
            asp1.transform.position = new Vector3(posX, posY, 0f);
            asp1.transform.localScale = new Vector3(scale, scale, 1f);
            asp1.SetActive(true);
        }
        private void createDreamFlash2(float posX, float posY, float scale)
        {
            var asp1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["DreamFlash3"]);
            asp1.transform.position = new Vector3(posX, posY, 0f);
            asp1.transform.localScale = new Vector3(scale, scale, 1f);
            asp1.SetActive(true);
        }*/
        private void createShockwave(bool faceRight, float posX, float posY, float scale, float speed, int damage)
        {
            PlayMakerFSM lord = Hollowed_Mode.preloadedGO["Soul Master"].LocateMyFSM("Mage Lord");
            GameObject go = GameObject.Instantiate(lord.GetAction<SpawnObjectFromGlobalPool>("Quake Waves", 0).gameObject.Value);
            go.transform.localScale = new Vector2(scale, scale);
            PlayMakerFSM shock = go.LocateMyFSM("shockwave");
            shock.FsmVariables.FindFsmBool("Facing Right").Value = faceRight;
            shock.FsmVariables.FindFsmFloat("Speed").Value = speed;
            //shock.FsmVariables.FindFsmInt("Damage").Value = 2;
            go.SetActive(true);
            go.transform.SetPosition2D(posX, posY);
        }
        private void createFocusBlast(float posX, float posY, float scale, int damage)
        {
            var asp1 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Focus Blast"]);
            asp1.transform.position = new Vector3(posX, posY, 0f);
            foreach (DamageHero x in asp1.GetComponentsInChildren<DamageHero>(true))
                x.damageDealt = damage;
            asp1.transform.localScale = new Vector3(scale, scale, 1f);
            asp1.SetActive(true);
        }
        void changeAspidMotherFSM(string objectname, int health, int babyHealth)
        {
            var asp1 = GameObject.Find(objectname);
            PlayMakerFSM fsm  = asp1.LocateMyFSM("Hatcher");
            fsm.ChangeTransition("Distance Fly", "WAIT", "Fire Anticipate");
            fsm.GetAction<IntCompare>("Hatched Max Check", 0).integer1 = 0;
            fsm.GetAction<IntCompare>("Hatched Max Check", 0).integer2 = 999;
            fsm.InsertMethod("Fire", 0,() => addPrimalAspid(asp1.transform.position.x, asp1.transform.position.y - 1f, babyHealth, 0.8f));
            asp1.GetComponent<HealthManager>().hp = health;
        }
        private void gruzBirth(float posX, float posY, int health, bool isAscended)
        {
            counter1++;
            if (counter1 == 2)
            {
                if(isAscended) addPrimalAspid(posX, posY, health, 1f);
                else addGruzzer(posX, posY, health);
                counter1 = 0;
            }
        }
        private void starvingMawlek(float posX, float posY)
        {
            counter1++;
            Log("Counter at the moment is at" + counter1);
            if (counter1 == 3)
            {
                if(isBossfight == false)
                    addVengefly(posX, posY, 9);
                else
                    addVengefly(posX, posY, 42);
            }

            if (counter1 == 6)
            {
                if(isBossfight == false)
                    addObble(posX, posY, 10);
                else
                    addObble(posX, posY, 42);
            }

            if (counter1 == 9)
            {
                if(isBossfight == false)
                    addVengefly(posX, posY, 9);
                else
                    addVengefly(posX, posY, 42);
            }

            if (counter1 == 12)
            {
                if(isBossfight == false)
                    addAspidHunter(posX, posY);
                else
                    addPrimalAspid(posX, posY, 55, 1f);
            }

            if (counter1 == 15)
            {
                if(isBossfight == false)
                    addObble(posX, posY, 10);
                else
                    addObble(posX, posY, 42);
            }
            if (counter1 > 17)
            {
                if(isBossfight == false)
                    addAspidHunter(posX, posY);
                else
                    addPrimalAspid(posX, posY, 55, 1f);
                counter1 = 0;
            }
        }
        private void changeGruzMotherFSM(string objectname, int babyHealth, int addHealth, bool isAscended)
        {
            var boss = GameObject.Find(objectname);
            foreach (DamageHero x in boss.GetComponentsInChildren<DamageHero>(true))
                x.damageDealt = 2;
            boss.GetComponent<HealthManager>().hp += addHealth;
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Big Fly Control");
            bossFsm.InsertMethod("Charge", 0,() => addGruzzer(boss.transform.position.x, boss.transform.position.y, babyHealth));
            bossFsm.InsertMethod("Charge", 0,() => addGruzzer(boss.transform.position.x, boss.transform.position.y, babyHealth));
            bossFsm.InsertMethod("Slam Up", 0,() => gruzBirth(boss.transform.position.x, boss.transform.position.y, babyHealth, isAscended));
            if (isAscended) bossFsm.InsertMethod("Recover End", 0,() => addPrimalAspid(boss.transform.position.x, boss.transform.position.y, babyHealth, 1f));
            else bossFsm.InsertMethod("Recover End", 0,() => addGruzzer(boss.transform.position.x, boss.transform.position.y, babyHealth));
            bossFsm.InsertMethod("Slam Down", 0,() => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 1.8f, 1.4f, 22f, 1));
            bossFsm.InsertMethod("Slam Down", 0,() => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 1.8f, 1.4f, 22f, 1));
        }
        /*private void changeVengeflyKingFSM(string objectname)
        {
            var boss = GameObject.Find(objectname);
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Big Buzzer");
            Log("Single and ready to mingle");
            bossFsm.GetAction<SetDamageHeroAmount>("Intro Roar Antic", 6).damageDealt = 2;
            bossFsm.GetAction<IntCompare>("Check Summon", 4).integer1 = 10;
            bossFsm.GetAction<IntCompare>("Check Summon GG", 2).integer1 = 0;
            bossFsm.GetAction<FloatCompare>("Choose Attack", 1).float2 = 50f;
            bossFsm.GetAction<WaitRandom>("Idle", 7).timeMin = 0.2f;
            bossFsm.GetAction<WaitRandom>("Idle", 7).timeMax = 0.3f;
            bossFsm.GetAction<SetVelocity2d>("Swoop L", 2).x = -30f;
            bossFsm.GetAction<Wait>("Swoop L", 4).time = 0.6f;
            bossFsm.GetAction<SetVelocity2d>("Swoop R", 2).x = 40f;
            bossFsm.GetAction<Wait>("Swoop R", 4).time = 0.4f;
        }*/
        private void changeFalseKnightFSM(string objectname)
        {
            var boss = GameObject.Find(objectname);
            PlayMakerFSM bossFsm = boss.LocateMyFSM("FalseyControl");
            bossFsm.InsertMethod("JA Antic", 0, () => counter1 = 0);
            bossFsm.InsertMethod("JA Jump", 0,() => falseKnightChainSlams(objectname));
            bossFsm.GetAction<SetVelocity2d>("JA Jump", 4).y = 50;
            bossFsm.InsertMethod("JA Slam", 0,() => createShockwave(false, boss.transform.position.x, boss.transform.position.y, 2.5f, 22f, 1));
            bossFsm.InsertMethod("JA Slam", 0,() => createShockwave(true, boss.transform.position.x, boss.transform.position.y, 1.2f, 22f, 1));
            bossFsm.InsertMethod("JA Slam", 0,() => Log("Shockwaves Spawned and Chain Slam "+counter1));
            bossFsm.GetAction<SetVelocity2d>("Jump", 6).y = 50;
        }
        private void falseKnightChainSlams(string objectname)
        {
            counter1++;
            var boss = GameObject.Find(objectname);
            PlayMakerFSM bossFsm = boss.LocateMyFSM("FalseyControl");
            if(counter1 == 0) bossFsm.ChangeTransition("JA Slam", "FINISHED", "JA Jump");
            if(counter1 == 3) bossFsm.ChangeTransition("JA Slam", "FINISHED", "JA Recoil");
        }
        private void changeBroodingMawlekFSM(string objectname)
        {
            var boss = GameObject.Find(objectname);
            var bossMouth = GameObject.Find(objectname+"/Mawlek Head");
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Mawlek Control");
            foreach (DamageHero x in boss.GetComponentsInChildren<DamageHero>(true))
                x.damageDealt = 2;
            PlayMakerFSM headFsm = bossMouth.LocateMyFSM("Mawlek Head");
            bossFsm.InsertMethod("Land", 0,() => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 2.2f,1.4f, 22f, 1));
            bossFsm.InsertMethod("Land", 0,() => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 2.2f,1.4f, 22f, 1));
            bossFsm.InsertMethod("Land 2", 0,() => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 2.2f,1.4f, 22f, 1));
            bossFsm.InsertMethod("Land 2", 0,() => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 2.2f,1.4f, 22f, 1));
            headFsm.GetAction<Wait>("Shoot Antic", 1).time = 0.041f;
            headFsm.InsertMethod("Shoot", 0,() => starvingMawlek(boss.transform.position.x, boss.transform.position.y + 3f));
        }
        /*private void changeMassiveMossChargerFSM(string objectname, int plusHeath)
        {
            var boss = GameObject.Find(objectname);
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Mossy Control");
            foreach (DamageHero x in boss.GetComponentsInChildren<DamageHero>(true))
                x.damageDealt = 2;
            boss.GetComponent<HealthManager>().hp += plusHeath;
            bossFsm.GetAction<SetVelocity2d>("Hit Right", 2).x = 0;
            bossFsm.GetAction<SetVelocity2d>("Hit Left", 2).x = 0;
            bossFsm.GetAction<Wait>("Hit Right", 1).time = 0;
            bossFsm.GetAction<Wait>("Hit Left", 1).time = 0;
            bossFsm.InsertMethod("Leap Launch", 0,() => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 4.02f, 1.4f, 22f,1)); 
            bossFsm.InsertMethod("Leap Launch", 0,() => Log("Boss Launch Position is: " + boss.transform.position.x + " " + boss.transform.position.y)); 
            bossFsm.InsertMethod("Leap Launch", 0, () => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 4.02f, 1.4f, 22f, 1));
            bossFsm.InsertMethod("Land", 0, () => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 3.53f, 3.1f, 22f, 1));
            bossFsm.InsertMethod("Land", 0, () => Log("Boss land position is: " + boss.transform.position.x + " " + boss.transform.position.y));
            bossFsm.InsertMethod("Land", 0, () => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 3.53f, 3.1f, 22f, 1));
            bossFsm.InsertMethod("Submerge 3", 0, () => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 3.92f, 0.5f, 22f, 1));
            bossFsm.InsertMethod("Submerge 3", 0, () => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 3.92f, 0.5f, 22f, 1));
            bossFsm.InsertMethod("Submerge 3", 0, () => Log("Boss submerge position is: " + boss.transform.position.x + " " + boss.transform.position.y));
        }
        private void changeMantisLordFSM(string objectname, int healthPlus, bool secondPhase)
        {
            var boss = GameObject.Find(objectname);
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Mantis Lord");
            boss.GetComponent<HealthManager>().hp += healthPlus;
            bossFsm.GetAction<Wait>("Idle", 0).time.Value = 0f;
            bossFsm.GetAction<Wait>("Start Pause", 0).time.Value = 0;
            bossFsm.GetAction<Wait>("Throw CD", 0).time.Value = 0;
            if (secondPhase == false)
            {
                bossFsm.GetAction<Wait>("Arrive", 4).time.Value /= 3;
                bossFsm.GetAction<Wait>("Leave Pause", 0).time.Value /= 2;
                bossFsm.GetAction<Wait>("After Throw Pause", 3).time.Value /= 2;
            }
            if (secondPhase == true)
            {
                bossFsm.GetAction<Wait>("Arrive", 4).time.Value *= 1.1f;
                bossFsm.GetAction<Wait>("Leave Pause", 0).time.Value *= 1.8f;
                bossFsm.GetAction<Wait>("After Throw Pause", 3).time.Value *= 1.8f;
            }
            bossFsm.FsmVariables.FindFsmFloat("Dash Speed").Value = 80f;
            float speedModifier = 2f;
            if (loadedOnce == true)
            {
                foreach (var i in boss.GetComponent<tk2dSpriteAnimator>().Library.clips)
                    i.fps *= 1/speedModifier;
            }
            foreach (var i in boss.GetComponent<tk2dSpriteAnimator>().Library.clips)
                i.fps *= speedModifier;
            loadedOnce = true;
        }
        private void changeMarmuFSM(string objectname)
        {
            var boss = GameObject.Find(objectname);
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Control");
            bossFsm.GetAction<SetVelocity2d>("Hit Right", 0).x = 0;
            bossFsm.GetAction<SetVelocity2d>("Hit Right", 0).y = 0;
            bossFsm.GetAction<SetVelocity2d>("Hit Left", 0).x = 0;
            bossFsm.GetAction<SetVelocity2d>("Hit Left", 0).y = 0;
            bossFsm.GetAction<SetVelocity2d>("Hit Up", 0).x = 0;
            bossFsm.GetAction<SetVelocity2d>("Hit Up", 0).y = 0;
            bossFsm.GetAction<SetVelocity2d>("Hit Down", 0).x = 0;
            bossFsm.GetAction<SetVelocity2d>("Hit Down", 0).y = 0;
        }
        private void changeElderHuFSM(string objectname)
        {
            var boss = GameObject.Find(objectname);
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Attacking");
            bossFsm.GetAction<WaitRandom>("Wait", 0).timeMin = 0.1f;
            bossFsm.GetAction<WaitRandom>("Wait",0).timeMax = 0.2f;
            bossFsm.FsmVariables.FindFsmFloat("Mega Pause").Value = 0.1f;
        }
        private void changeCrystalGuardianFSM(string objectname)
        {
            var boss = GameObject.Find(objectname);
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Beam Miner");
            bossFsm.GetAction<Wait>("Beam Antic",14).time = 0.2f;
            bossFsm.InsertMethod("Roar", 0,() => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 2.4f, 4.5f, 11f, 3));
            bossFsm.InsertMethod("Roar", 0,() => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 2.4f, 4.5f, 11f, 3));
            bossFsm.InsertMethod("Land", 0,() => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 2.4f, 1.4f, 22f, 3));
            bossFsm.InsertMethod("Land", 0,() => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 2.4f, 1.4f, 22f, 3));
        }
        private void changeMageKnightFSM(string objectname)
        {
            var boss = GameObject.Find(objectname);
            PlayMakerFSM bossFsm = boss.LocateMyFSM("Mage Knight");
            bossFsm.GetAction<WaitRandom>("Idle", 5).timeMin = 0.01f;
            bossFsm.GetAction<WaitRandom>("Idle",5).timeMax = 0.1f;
            bossFsm.InsertMethod("Stomp Recover", 0, () => createShockwave(false, boss.transform.position.x, boss.transform.position.y - 3.4f, 2.5f, 11f, 3));
            bossFsm.InsertMethod("Stomp Recover", 0, () => createShockwave(true, boss.transform.position.x, boss.transform.position.y - 3.4f, 2.5f, 11f, 3));
        }*/

        //-----------------------------------------------------------------------------------------------------
        //                                               Room Changes
        //-----------------------------------------------------------------------------------------------------
        
        private void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            if (to.name == "Tutorial_01") //King's Pass
            {
                tabletNumber = 0;
                var tablet = GameObject.Find("_Props/Tut_tablet_top (2)");
                tablet.transform.position = new Vector3(94.7f, 14.5f, 3.5f);
                var children1 = tablet.GetComponentsInChildren<SpriteRenderer>();
                foreach (var child1 in children1)
                {
                    if (child1.name == "lit_tablet")
                    {
                        child1.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0f);
                    }
                }
                addSpkCollision(112.3f, 8.9f, 8f, 1f);
                addPlatHeavy(109.4f, 12.7f, 1f, 1f);
                addPlatHeavy(108.0f, 14.0f, 1f, 1f);
                addZapCloud(112.3f, 11.8f, 1f, 1f);
                addSpike(109.4f, 8.6f, 0f);
                addSpike(110.4f, 8.6f, 0f);
                addSpike(111.7f, 8.6f, 0f);
                addSpike(113.0f, 8.6f, 0f);
                addSpike(114.3f, 8.6f, 0f);
                addSpike(115.6f, 10.6f, 0f);
                addSpike(116.9f, 10.6f, 0f);
                addSpike(118.2f, 10.6f, 0f);
                addSpike(119.5f, 10.6f, 0f);
                addSpike(120.8f, 10.6f, 0f);
                addSpike(122.1f, 10.6f, 0f);
                addSpike(123.5f, 10.6f, 0f);
                addSpike(123.8f, 10.6f, 0f);
                addSpkCollision(121.2f, 10.9f, 9.5f, 1f);
                addVengefly(119.4f, 15.6f, 9);
                addSentryNail(66.0f, 11.4f);
                addSentryNail(79.0f, 11.4f);
                addZapCloud(125.8f, 12.3f, 1f, 1f);
                addSentrySpear(137.5f, 15.2f);
                addSpkCollision(138.2f, 7.0f, 5f, 1f);
                addSpike(136.4f, 6.7f, 0f);
                addSpike(137.7f, 6.7f, 0f);
                addSpike(139.0f, 6.7f, 0f);
                addSpkCollision(149f, 4.9f, 12f, 1f);
                addSpike(140.3f, 4.7f, 0f);
                addSpike(141.6f, 4.7f, 0f);
                addSpike(142.9f, 4.7f, 0f);
                addSpike(144.2f, 4.7f, 0f);
                addSpike(145.5f, 4.7f, 0f);
                addSpike(146.8f, 4.7f, 0f);
                addSpike(148.1f, 4.7f, 0f);
                addSpike(149.4f, 4.7f, 0f);
                addSpike(150.7f, 4.7f, 0f);
                addSpike(152.0f, 4.7f, 0f);
                addSpike(152.7f, 4.7f, 0f);
                addSpkCollision(141.8f, 11.6f, 3.5f, 1f);
                addSpike(140.3f, 11.4f, 0f);
                addSpike(141.7f, 11.4f, 0f);
                addZapCloud(141.1f, 15.9f, 1f, 1f);
                addZapCloud(138.8f, 24.5f, 1f, 1f);
                var tutSpikeC6 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["SpikeCol"]);
                tutSpikeC6.transform.position = new Vector3(145.7f, 22.2f, 0f);
                tutSpikeC6.transform.SetScaleX(2.1f);
                tutSpikeC6.SetActive(true);
                addSpike(145.7f, 22.1f, 0f);
                addSpike(144.7f, 22.1f, 0f);
                addZapCloud(145.4f, 24.4f, 1f, 1f);
                addZapCloud(145.4f, 24.4f, 1f, 1f);
                addSpkCollision(153f, 9.2f, 2.1f, 1f);
                addSpike(152.8f, 9.2f, 0f);
                addSentryNail(114f, 31f);
                addSentryNail(108f, 63.7f);
                addSpkCollision(60.8f, 29.3f, 2.1f, 1f);
                var tutSpike33 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Spikes"]);
                tutSpike33.transform.position = new Vector3(60.3f, 29.3f, 0f);
                tutSpike33.transform.SetScaleX(1.1f);
                tutSpike33.SetActive(true);
                addSpkCollision(62.8f, 37.0f, 3.0f, 1f);
                var tutSpike34 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["Spikes"]);
                tutSpike34.transform.position = new Vector3(62.5f, 37.0f, 0f);
                tutSpike34.transform.SetScaleX(1.5f);
                tutSpike34.SetActive(true);
                addZapCloud(67.5f, 34.7f, 1f, 1f);
                addSentrySpear(67.2f, 45.6f);
                addSentrySpear(59.9f, 34.6f);
                addSentrySpear(80.3f, 52.7f);
                var tutPlat3 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["LongPlat"]);
                tutPlat3.transform.position = new Vector3(140.5f, 44.7f, 0f);
                tutPlat3.transform.SetScaleY(5f);
                tutPlat3.SetActive(true);
                var tutPlat4 = GameObject.Instantiate(Hollowed_Mode.preloadedGO["LongPlat"]);
                tutPlat4.transform.position = new Vector3(148.8f, 44.7f, 0f);
                tutPlat4.transform.SetScaleY(5f);
                tutPlat4.SetActive(true);
                addZapCloud(144.8f, 42.4f, 1f, 1f);
                addZapCloud(152.8f, 42.4f, 1f, 1f);
                addSentrySpear(177.3f, 41.5f);
                addSentrySpear(177.3f, 41.5f);
                addZombieGuard(187.2f, 27.4f, 0);
                addZombieGuard(185.6f, 69.4f, 0);
                addSentrySpear(185.6f, 72.5f);
                addSentrySpear(184.6f, 69.5f);
                addLocalSwitch(61.8f, 46.6f, "gate1", 180f, 0, Settings.beenToCrossroads);
                if(Settings.beenToCrossroads == false) addLocalSwitchGate(151.2f, 58.5f, "gate1", 1f, 1f, true);
                addLocalSwitch(195.8f, 21.6f, "gate2", 180f, 0, Settings.beenToCrossroads);
                if(Settings.beenToCrossroads == false) addLocalSwitchGate(148.2f, 58.5f, "gate2", 1f, 1f, true);
            }
            if (to.name == "Town") //Dirtmouth
            {
                var dirtmouthSentry = GameObject.Instantiate(Hollowed_Mode.preloadedGO["ZombieGuard"]);
                dirtmouthSentry.transform.position = new Vector3(219.2f, 25.4f, 0f);
                dirtmouthSentry.SetActive(true);
                dirtmouthSentry.GetComponent<HealthManager>().hp = 200;
                foreach (var i in dirtmouthSentry.GetComponent<tk2dSpriteAnimator>().Library.clips)
                    i.fps *= 2.2f;
                if (Settings.beenToCrossroads == true) Log("Has been to Crossroads");
                else Log("Has not been to Crossroads");
            }
            //Forgotten Crossroads
            if (to.name == "Crossroads_01") //Entrance
            {
                Settings.beenToCrossroads = true;
                addSpkCollision(78.6f, 11.7f, 3.7f, 1f);
                addSpike(76.5f, 11.5f, 0f);
                addSpike(77.8f, 11.5f, 0f);
                addSpike(79.1f, 11.5f, 0f);
                addLocalSwitch(77.8f, 18.8f, "gate1", 0f, 1, Settings.CrossroadsLever1);
                if(Settings.CrossroadsLever1 == false) addLocalSwitchGate(2.0f, 8.5f, "gate1", 1f, 1f, true);
                addLocalSwitch(9.1f, 4.5f, "gate2", 180f, 0, Settings.CrossroadsLever1);
                if(Settings.CrossroadsLever1 == false) addLocalSwitchGate(60.5f, 11.5f, "gate2", 1f, 1f, true);
                addZapCloud(77.5f, 15.7f, 1f, 1f);
                addZapCloud(85.5f, 14.7f, 1f, 1f);
                addPlatHeavy(82.2f, 16.4f, 1f, 1f);
                addBigAspid(15.3f, 16.6f);
                addAspidHunter(32.3f, 17.6f);
                addAspidHunter(21.3f, 16.9f);
                addAspidHunter(15.3f, 16.9f);
                addSpkCollision(29.3f, 10.0f, 16.4f, 1f);
                addSpike(18.4f, 9.6f, 0f);
                addSpike(19.7f, 9.6f, 0f);
                addSpike(21.0f, 9.6f, 0f);
                addSpike(22.3f, 9.6f, 0f);
                addSpike(23.6f, 9.6f, 0f);
                addSpike(24.9f, 9.6f, 0f);
                addSpike(26.2f, 9.6f, 0f);
                addSpike(27.5f, 9.6f, 0f);
                addSpike(28.8f, 9.6f, 0f);
                addSpike(30.1f, 9.6f, 0f);
                addSpike(31.4f, 9.6f, 0f);
                addSpike(32.7f, 9.6f, 0f);
                addSpike(34.0f, 9.6f, 0f);
                addSpkCollision(36.3f, 6.0f, 2f, 1f);
                addSpike(36.3f, 5.6f, 0f);
                addSpkCollision(43.3f, 2.0f, 20f, 1f);
                addSpike(37.8f, 1.6f, 0f);
                addSpike(39.1f, 1.6f, 0f);
                addSpike(40.4f, 1.6f, 0f);
                addSpike(41.7f, 1.6f, 0f);
                addSpike(43.0f, 1.6f, 0f);
                addSpike(44.3f, 1.6f, 0f);
                addSpike(45.6f, 1.6f, 0f);
                addSpike(46.9f, 1.6f, 0f);
                addSpike(48.2f, 1.6f, 0f);
                addZapCloud(83.3f, 6.9f, 5f, 1f);
                addZapCloud(69.0f, 13.3f, 1f, 1f);
            }
            if (to.name == "Crossroads_07") //Tall Room
            {
                timedSwitch = false;
                counter1 = 35;
                counter2 = 15;
                foreach (var gameobject in to.GetRootGameObjects())
                {
                    if (gameobject.name == "_Enemies")
                    {
                        var children = gameobject.GetComponentsInChildren<Transform>();
                        foreach (var child in children)
                        {
                            if (child.name.Contains("Climber") || child.name.Contains("Crawler"))
                            {
                                UnityEngine.Object.Destroy(child.gameObject);
                            }
                        }
                    }
                }
                addPlatLift(26.5f+5.95f, 66.6f+10.36f, 12f, 1f, 1f);
                addPlatLift(26.5f-20.38f, 66.6f+5.81f, -24f, 1f, 1f);
                addPlatLift(26.5f-1.66f, 66.6f+3.79f, -90f, 1f, 1f);
                addPlatLift(26.5f-19.54f, 66.6f-6.03f, 40f, 1f, 1f);
                addPlatLift(26.5f-12.04f, 66.6f-3.42f, 28f, 0.6603901f, 1.048701f);
                addPlatLift(26.5f-4.32f, 66.6f-6.34f, -70f, 1f, 1f);
                addPlatLift(26.5f+4.33f, 66.6f-7.86f, -90f, 1f, 1.5f);
                addPlatLift(26.5f+9.36f, 66.6f-0.2f, -75f, 1.2f, 1f);
                addPlatLift(26.5f+4.84f, 66.6f-27.81f, 0f, 1f, 1f);
                addPlatLift(26.5f-3.69f, 66.6f-25.15f, -70f, 1f, 1f);
                addPlatLift(26.5f-13.28f, 66.6f-32.12f, 0f, 1f, 1f);
                addPlatLift(26.5f-4.06f, 66.6f-36.852f, -90f, 2.02f, 1f);
                addPlatLift(26.5f-7.67f, 66.6f-48.81f, 0f, 1f, 1f);
                addAspidMother(28f, 87f, counter1, counter2);
                addAspidMother(16f, 85f, counter1, counter2);
                addAspidMother(12f, 68f, counter1, counter2);
                addAspidMother(36f, 74f, counter1, counter2);
                addAspidMother(16f, 59.2f, counter1, counter2);
                addAspidMother(28f, 70.7f, counter1, counter2);
                addBigAspid(28.4f, 70.7f);
                addBigAspid(21.8f, 83.9f);
                addBigAspid(21f, 70f);
                addBigAspid(34f, 74f);
                addAspidTurret(22.3f, 66.4f, 35, false);
                addAspidTurret(35.6f, 74.1f, 35, false);
                addAspidTurret(37.9f, 61.8f, 35, false);
                addAspidMother(28f, 87f, counter1, counter2);
                addBigAspid(17.6f, 49f);
                addAspidTurret(22.5f, 38.3f, 35, false);
                addAspidMother(33.4f, 34f, counter1, counter2);
                addAspidMother(11.4f, 44f, counter1, counter2);
                addBigAspid(27f, 28f);
                addAspidMother(34.3f, 12.7f, counter1, counter2);
                addAspidMother(25.6f, 21f, counter1, counter2);
                addAspidMother(14.6f, 24f, counter1, counter2);
                addBigAspid(16f, 24f);
                addAspidMother(6.6f, 24f, counter1, counter2);
                addBigAspid(13.6f, 13.7f);
                addBigAspid(18.6f, 14.5f);
                addBigAspid(7.5f, 14.4f);
                addAspidMother(10.8f, 12f, counter1, counter2);
                addPrimalAspid(23.4f, 46.4f, counter2, 0.7f);
                addThornSmall(28f, 3.0f, "2", 90f, 1.4f);
                addThornSmall(32f, 3.7f, "2", 90f, 1f);
                addThornSmall(36f, 3.0f, "2", 90f, 1.4f);
                addThornSmall(40f, 3.7f, "2", 90f, 1f);
                addThornCollision(11f, 3.6f, 6.5f, 1.9f, 272f);
                addThornSmall(5f, 3.7f, "2", 90f, 1f);
                addThornSmall(9f, 3.0f, "2", 90f, 1.4f);
                addThornSmall(13f, 3.7f, "2", 90f, 1f);
                addThornCollision(0f, 4.3f, 2.5f, 1.9f, 272f);
                addThornCollision(-3f, 4.3f, 2.5f, 1.9f, 272f);
                if(Settings.CrossroadsLever3 == false) addLocalSwitchGate(37.1f, 43.5f, "gate1", 1f, 1.2f, true);
                addLocalSwitch(41.6f, 43.2f, "gate1", 180f, 4, Settings.CrossroadsLever3);
            }
            if (to.name == "Crossroads_33") //Cornifer
            {
                addLocalSwitch(6.0f, 34.4f, "gate1", 180f, 2, Settings.CrossroadsLever2);
                addLocalSwitch(6.0f, 34.4f, "gate2", 180f, 2, Settings.CrossroadsLever2);
                addLocalSwitch(6.0f, 34.4f, "gate3", 180f, 2, Settings.CrossroadsLever2);
                if(Settings.CrossroadsLever2 == false) addLocalSwitchGate(10.8f, 34.4f, "gate1", 1f, 2.6f, true);
                if(Settings.CrossroadsLever2 == false) addLocalSwitchGate(26.4f, 28.6f, "gate2", 1f, 1f, true);
                if(Settings.CrossroadsLever2 == false) addLocalSwitchGate(19.4f, 28.6f, "gate3", 1f, 1f, true);
                if(timedSwitch == false) addLocalSwitchGate(42.8f, 9.4f, "invalid", 1f, 1.4f, true);
                addZapCloud(34.1f, 21.3f, 1.5f, 1.5f);
                addBigAspid(21.7f, 13.6f);
                addAspidMother(30f, 13.4f, 35, 18);
            }
            if (to.name == "Crossroads_38")
            {
                addBench(59.2f, 3.3f, -0.1f);
            }
            if (to.name == "Crossroads_25") //Brooding Mawlek Path
            {
                addZapCloud(10.3f, 17.8f, 1f, 1f);
                addZapCloud(6.5f, 11.6f, 0.5f, 0.5f);
                addZapCloud(14.1f, 11.6f, 0.5f, 0.5f);
                addPlatHeavy(57.1f, 10.8f, 1f, 1f);
                addPlatHeavy(57.1f, 12.8f, 1f, 1f);
                addBigAspid(51.3f, 6.9f);
                addBigAspid(36.3f, 8.1f);
                addBigAspid(39.3f, 14.9f);
            }
            if (to.name == "Crossroads_36") //Before Brooding Mawlek
            {
                addBench(12.9f, 30.7f, 0.1f);
                foreach (var gameobject in to.GetRootGameObjects())
                {
                    if (gameobject.name == "_Scenery")
                    {
                        var children = gameobject.GetComponentsInChildren<Transform>();
                        foreach (var child in children)
                        {
                            if (child.name.Contains("plat_float") && child.position.x > 31f)
                            {
                                UnityEngine.Object.Destroy(child.gameObject);
                            }
                        }
                    }
                }
                addPlatHeavy(54.1f, 41.8f, 1f, 1f);
                addPlatHeavy(49.0f, 35.0f, 1f, 1f);
                addSpkCollision(58.3f, 37.2f, 7f, 1f);
                addSpike(54.1f, 36.6f, 0f);
                addSpike(55.4f, 36.6f, 0f);
                addSpike(56.7f, 36.6f, 0f);
                addSpike(58.0f, 36.6f, 0f);
                addBigAspid(40.5f, 42.2f);
                addBigAspid(43.3f, 26.9f);
                addBigAspid(43.3f, 23.9f);
                addVengefly(14.8f, 22.6f, 9);
                addObble(51.8f, 52.6f, 10);
                addObble(51.8f, 52.6f, 10);
                addDirectionPoleBench(39f, 10.6f, 0f, 0f);
            }
            if (to.name == "Crossroads_09") //Brooding Mawlek Boss Room
            {
                timedSwitch = false;
                counter1 = 0;
                tabletNumber = 1;
                changeBroodingMawlekFSM("_Enemies/Mawlek Body");
                addTablet(29.2f, 12.2f, 4.5f, 0.9f, 1f, 0.9f);
            }
            if (to.name == "Crossroads_12") //Left of Cornifer
            {
                addBench(63f, 10f, -0.1f);
                foreach (var gameobject in to.GetRootGameObjects())
                {
                    if (gameobject.name == "_Scenery")
                    {
                        var children = gameobject.GetComponentsInChildren<Transform>();
                        foreach (var child in children)
                        {
                            if (child.name.Contains("plat_float_01"))
                            {
                                UnityEngine.Object.Destroy(child.gameObject);
                            }
                        }
                    }
                }
                addBigAspid(50f, 13.8f);
                addBigAspid(22.5f, 15.4f);
                var spikes1 = GameObject.Find("Cave Spikes Invis");
                spikes1.transform.position = new Vector3(0f, -3f, 0f);
                var spikes2 = GameObject.Find("Cave Spikes Invis (1)");
                spikes2.transform.position = new Vector3(0f, -3f, 0f);
                var spikes3 = GameObject.Find("Cave Spikes Invis (2)");
                spikes3.transform.position = new Vector3(0f, -3f, 0f);
                var spikes4 = GameObject.Find("Cave Spikes Invis (3)");
                spikes4.transform.position = new Vector3(0f, -3f, 0f);
                var spikes5 = GameObject.Find("Cave Spikes Invis (4)");
                spikes5.transform.position = new Vector3(0f, -3f, 0f);
                addGoamWorm(21.4f, 4.1f, 1f, 0f, 1f, true);
                addThornCollision(-10f, 4.3f, 6.9f, 1.9f, 272f);
            }
            if (to.name == "Crossroads_35") //Pathway between Fog Canyon and Crossroads
            {
                tabletNumber = 2;
                changeAspidMotherFSM("_Enemies/Hatcher", 35, 15);
                var bottle = GameObject.Find("Grub Bottle");
                bottle.transform.position = new Vector3(52.9f-4f, 11.4f-44.5f, 0f);
                addLocalSwitch(6f, 44f, "gate1", 180f, 5, timedSwitch);
                addTablet(31.2f, 43.4f, 4.5f, 255f, 45f, 0f);
            }
            if (to.name == "Crossroads_08") //Aspid Wave Fight
            {
                addBigAspid(31f, 24f);
                addBigAspid(39f, 13f);
                timedSwitch = true;
            } 
            if (to.name == "Crossroads_13") //Goams First Room
            {
                
                addThornSmall(27f, 4.7f, "2", 90f, 1f);
                addThornSmall(30f, 4.0f, "2", 90f, 1.4f);
                addThornSmall(34f, 4.7f, "2", 90f, 1f);
                addThornSmall(37f, 4.0f, "2", 90f, 1.4f);
                addThornSmall(40f, 4.7f, "2", 90f, 1f);
                addThornSmall(43f, 4.0f, "2", 90f, 1.4f);
                addThornSmall(46f, 4.7f, "2", 90f, 1f);
                addThornSmall(48f, 4.0f, "2", 90f, 1.4f);
                addThornCollision(12f, 4.3f, 6.5f, 1.9f, 272f);
            }
            if (to.name == "Crossroads_42") //Goams Second Room
            {
                addThornSmall(78.7f, 2.7f, "2", 90f, 1f);
                addThornSmall(75.5f, 2.0f, "2", 90f, 1.4f);
                addThornSmall(81.5f, 2.0f, "2", 90f, 1.4f);
                addThornCollision(66f, 2.3f, 3.2f, 1.9f, 272f);
                addThornSmall(88.8f, 2.7f, "2", 90f, 1f);
                addThornSmall(89.7f, 2.7f, "2", 90f, 1f);
                addThornSmall(92.7f, 2.7f, "2", 90f, 1f);
                addThornCollision(80f, 2.3f, 3.2f, 1.9f, 272f);
                addPlatHeavy(71f, 19f, 1.4f, 1.2f);
                addPlatHeavy(73f, 21f, 1.0f, 1.0f);
            }
            if (to.name == "Crossroads_04") //Salubra & Gruz Mother
            {
                counter1 = -3;
                changeGruzMotherFSM("Giant Fly", 10, 110, false);
            }
            if (to.name == "Crossroads_48") //Crossroads Warden & Grub Room
            {
                runEveryFrameScript = true; 
                frameScriptNumber = 1;
                frameCounter1 = 0;
                addBench(9.1f, 3.4f, -0.5f);
            }
            if (to.name == "Crossroads_14")
            {
                runEveryFrameScript = false;
            }
            if (to.name == "Crossroads_10") //False Knight Battle Room
            {
                isBossfight = true;
                bossNumber = 6;
            }
            //Godhome Bossfights
            if (to.name == "GG_Workshop")
            {
                runEveryFrameScript = false;
                isBossfight = false;
                bossNumber = 0;
            }
            /*if (to.name == "GG_Gruz_Mother")
            {
                counter1 = 0;
                changeGruzMotherFSM("Giant Fly", 40, 100, false);
            }
            if (to.name == "GG_Gruz_Mother_V")
            {
                counter1 = 1;
                changeGruzMotherFSM("Giant Fly", 60, 200, true);
            }
            if (to.name == "GG_Vengefly")
            {
                changeVengeflyKingFSM("Giant Buzzer Col");
            } 
            if (to.name == "GG_Vengefly_V")
            {
                changeVengeflyKingFSM("Giant Buzzer Col");
                changeVengeflyKingFSM("Giant Buzzer Col (1)");
            }
            if (to.name == "GG_Brooding_Mawlek")
            {
                isBossfight = true;
                counter1 = 0;
                counter2 = 0;
                bossNumber = 1;
            }
            if (to.name == "GG_Brooding_Mawlek_V")
            {
                isBossfight = true;
                counter1 = 0;
                counter2 = 0;
                bossNumber = 1;
            }
            if (to.name == "GG_Mega_Moss_Charger")
            {
                changeMassiveMossChargerFSM("Mega Moss Charger", 50);
                Log("Moss Charger Loaded and Ready");
            }
            if (to.name == "GG_Crystal_Guardian")
            {
                changeCrystalGuardianFSM("Mega Zombie Beam Miner (1)");
            }
            if (to.name == "GG_Mantis_Lords")
            {
                var boss = GameObject.Find("Mantis Battle/Battle Main/Mantis Lord");
                float speedModifier = 2f;
                if (loadedOnce == true)
                {
                    foreach (var i in boss.GetComponent<tk2dSpriteAnimator>().Library.clips)
                        i.fps *= 1/speedModifier;
                }
                foreach (var i in boss.GetComponent<tk2dSpriteAnimator>().Library.clips)
                    i.fps *= speedModifier;
                loadedOnce = true;
                changeMantisLordFSM("Mantis Battle/Battle Main/Mantis Lord", 100, false);
                changeMantisLordFSM("Mantis Battle/Battle Sub/Mantis Lord S1", 0, true);
                changeMantisLordFSM("Mantis Battle/Battle Sub/Mantis Lord S2", 0, true);

            }
            if (to.name == "GG_Mage_Knight")
            {
                changeMageKnightFSM("Mage Knight");
                isBossfight = true;
                counter1 = 23;
                bossNumber = 2;
            }
            if (to.name == "GG_Mage_Knight_V")
            {
                changeMageKnightFSM("Balloon Spawner/Mage Knight");
                isBossfight = true;
                counter1 = 0;
                bossNumber = 2;
            }
            if (to.name == "GG_Ghost_No_Eyes")
            {
                isBossfight = true;
                runEveryFrameScript = true;
                bossNumber = 4;
                frameScriptNumber = 0;
                frameCounter1 = 0;
                counter1 = 0;
                var boss = GameObject.Find("Warrior/Ghost Warrior No Eyes");
            }
            if (to.name == "GG_Ghost_No_Eyes_V")
            {
                isBossfight = true;
                runEveryFrameScript = true;
                bossNumber = 4;
                frameScriptNumber = 0;
                frameCounter1 = -60;
                counter1 = 0;
                var boss = GameObject.Find("Warrior/Ghost Warrior No Eyes");
            }
            if (to.name == "GG_Ghost_Marmu")
            {
                isBossfight = true;
                bossNumber = 5;
                counter2 = 0;
            }
            if (to.name == "GG_Ghost_Marmu_V")
            {
                isBossfight = true;
                bossNumber = 5;
                counter2 = 0;
            }
            if (to.name == "GG_Ghost_Xero")
            {
                addShadowGate(109f, 1f, 1f, 15f);
            }
            if (to.name == "GG_Ghost_Galien")
            {
                var scythe = GameObject.Find("Warrior/Galien Hammer");
                PlayMakerFSM scytheFSM = scythe.LocateMyFSM("Attack");
                scytheFSM.InsertMethod("Floor Bounce", 0, () => createShockwave(false, scythe.transform.position.x, 12.6f, 1.5f, 23f, 3));
                scytheFSM.InsertMethod("Floor Bounce", 0, () => createShockwave(true, scythe.transform.position.x, 12.6f, 1.5f, 23f, 3));
            }
            if (to.name == "GG_Ghost_Gorb")
            {
                addVoltTwister(HeroController.instance.transform.position.x, 100f, 100);
            }
            if (to.name == "GG_Ghost_Gorb_V")
            {
                addVoltTwister(HeroController.instance.transform.position.x, 100f, 100);
            }
            if (to.name == "GG_Ghost_Hu")
            {
                isBossfight = true;
                bossNumber = 3;
                counter2 = 0;
            }*/
        }
        public void EnemyHit(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hit)
        {
            orig(self, hit);
            if (isBossfight)
            {
                switch (bossNumber)
                {
                    /*//Brooding Mawlek - Set FSM properly
                    case 1:
                        if (counter2 < 1)
                        {
                            changeBroodingMawlekFSM("Battle Scene/Mawlek Body");
                            counter2++;
                        }
                        break;
                    //Mage Knights - Summon Extra Soul Warriors as the Fight goes on
                    case 2:
                        Log(counter1);
                        if(counter1 == 4) addMageKnight(HeroController.instance.transform.position.x, 6.4f, 750);
                        if(counter1 == 30) addMageKnight(HeroController.instance.transform.position.x, 6.4f, 750);
                        counter1++;
                        break;
                    //Elder Hu - Set FSM properly
                    case 3:
                        if (counter2 < 1)
                        {
                            changeElderHuFSM("Warrior/Ghost Warrior Hu");
                            counter2++;
                        }
                        break;
                    //No Eyes uses a massive Focus Blast as a get-off-me attack
                    case 4:
                        counter1++;
                        if (counter1 == 4)
                        {
                            counter1 = 0;
                            createFocusBlast(GameObject.Find("Warrior/Ghost Warrior No Eyes").transform.position.x, GameObject.Find("Warrior/Ghost Warrior No Eyes").transform.position.y, 2.5f, 3);
                        }
                        break;
                    //Set Marmu AI properly
                    case 5:
                        if (counter2 < 1)
                        {
                            changeMarmuFSM("Warrior/Ghost Warrior Marmu");
                            counter2++;
                        }
                        break;*/
                    case 6:
                        //changeFalseKnightFSM("Battle Scene/False Knight New");
                        Log("FSM Sucessfully set");
                        break;
                }
            }
        }
        public void OnHeroUpdate()
        {
            if (runEveryFrameScript == true && !HeroController.instance.cState.isPaused)
            {
                switch (frameScriptNumber)
                {
                    //No Eyes uses Focus Blasts to attack the knight
                    case 0:
                        frameCounter1++;
                        if (frameCounter1 == 200) createFocusBlast(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 1.1f, 2);
                        if (frameCounter1 == 400)
                        {
                            createFocusBlast(HeroController.instance.transform.position.x, 3f, 1.1f, 2);
                            createFocusBlast(HeroController.instance.transform.position.x, 9f, 1.1f, 2);
                            createFocusBlast(HeroController.instance.transform.position.x, 15f, 1.1f, 2);
                            createFocusBlast(HeroController.instance.transform.position.x, 21f, 1.1f, 2);
                            createFocusBlast(HeroController.instance.transform.position.x, 27f, 1.1f, 2);
                        }
                        if (frameCounter1 == 500) createFocusBlast(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 1.6f, 2);
                        if (frameCounter1 == 750) createFocusBlast(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0.7f, 2);
                        if (frameCounter1 == 780) createFocusBlast(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0.7f, 2);
                        if (frameCounter1 == 810) createFocusBlast(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0.8f, 2);
                        if (frameCounter1 == 840) createFocusBlast(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0.8f, 2);
                        if (frameCounter1 == 870) createFocusBlast(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0.9f, 2);
                        if (frameCounter1 == 900) createFocusBlast(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0.9f, 2);
                        if (frameCounter1 == 929) frameCounter1 = 199;
                        break;
                    //Mantis Lord in the Crossroads
                    case 1:
                        if (HeroController.instance.transform.position.x >= 24.4f && Settings.defeatedMLCrossroads1 == false && frameCounter1 == 0)
                        {
                            frameCounter1 = 1;
                            var tablet = GameObject.Find("TileMap Render Data/Scenemap/Chunk 0 0");
                            tablet.transform.position = new Vector3(0f, -16.4f, 0f);
                            var tablet2 = GameObject.Find("Roof Collider (1)");
                            tablet2.transform.Rotate(0f, 180f, 0f);
                            tablet2.transform.position = new Vector3(1.24f, 30.71f, 0.2f);
                            var tablet3 = GameObject.Find("Roof Collider (2)");
                            tablet3.transform.position = new Vector3(1.24f, 30.71f, 0.2f);
                            addTraitorLord(15.7f, 13f, 1.1f, 4, 1);
                        } 
                        break;
                }
            }
        }
        //-----------------------------------------------------------------------------------------------------
        //                                           Changing Game Text
        //-----------------------------------------------------------------------------------------------------
        public string TextDisplayInGame(string key, string sheet)
        {
            string origText = Language.Language.GetInternal(key, sheet);
            //Charm Name and Description Changes
            if (key == "CHARM_NAME_2")
                return "Compass of Insight";
            if (key == "CHARM_DESC_2")
                return "Whispers it's location to the bearer whenever a map is open, allowing wanderers to pinpoint their current location.<br>" +
                        "Tells it's wearer information about their progress that usually cannot be seen.<br><br>" +
                        "<color=yellow>Ancient's Essence found: " + Settings.AncientEssence + "<br>" +
                        "<color=red>Madman's Knowledge found: " + Settings.MadmanKnowledge + "<br>";
            if (key == "CHARM_NAME_4")
                return "Plated Shell";
            if (key == "CHARM_DESC_4")
                return "A shell forged from multiple armored plates, that break every time the knight takes a hit." +
                       "<br><br>Increases invincibility frames and reduces damage taken, however as the knight gets hit the damage he takes increases.";
            if (key == "CHARM_DESC_15")
                return "Formed from the nails of fallen warriors. <br>Increases the force of the bearer's nail, causing enemies to recoil further when hit and increasing damage by 20%.";
            if (key == "CHARM_DESC_25")
                return "Strengthens the bearer, increasing the damage they deal to enemies with their nail." +
                       "<br><br>This charm is fragile and weakens the wearer's shell.<br>It will break if its bearer is killed.";
            if (key == "CHARM_DESC_29")
                return "Golden nugget of the Hive's precious hardened nectar." +
                       "<br><br>Heals the bearer's wounds over time, allowing them to regain health, but the bearer loses the ability to focus their SOUL.";
            if (key == "CHARM_DESC_33")
                return "Reflecting the desires of the Soul Sanctum for mastery over SOUL, it improves the bearer's ability to cast spells. " +
                       "<br>Reduces the SOUL cost of casting spells, but also reduces nail damage by 20%";
            if (key == "CHARM_DESC_37")
                return "Bears the likeness of a strange bug known only as 'The Sprintmaster'." +
                       "<br><br>Increases movement speed and as your health drops and adrenaline rises, it increases it even further.";
            //Dialogue Changes
            if (key == "ELDERBUG_INTRO_MAIN")
                return
                    "The other residents, they've all disappeared. Headed down that well, one by one, into the caverns below.<br>" +
                    "Used to be there was a great kingdom beneath our town. It&'s long fell to ruin, yet it still draws folks into its depths.<br>" +
                    "Wealth, glory, enlightenment, that darkness seems to promise all things. I'm sure you too seek your dreams down there.<br>" +
                    "Well watch out. It's a sickly air that fills the place. Creatures turn mad and travellers are robbed of their memories.<br>" +
                    "Perhaps dreams aren't such great things after all...<br><br>" +
                    "Many used to come, hoping the kingdom would fulfill their desires.<br>Hallownest, it was once called. Supposedly the greatest kingdom there ever was, full of treasures and secrets.<br>Hm. Now it's nothing more than a poisonous tomb full of monsters and madness.<br>Everything fades eventually, I suppose.<br>" +
                    "Ah yes, this reminds me of a story when I was just a youngin, it was a summer night exactly 82 cycles ago...<br>" +
                    "Wait... what's that?<br><br>" +
                    "You don't have time to listen to my stories? Hmpf. This generation has no respect for it's elders.<br><br><br>" +
                    "Either way, if you want to talk with someone, I'm always here. You might be surprised by the things I have to say.";
            //Hunter's Journal Changes
            if (key == "NAME_MAWLEK")
                return "Starving Mawlek";
            if (key == "DESC_TRAITOR_LORD")
                return "Used to be one of the Lord of the Mantis tribe, long ago. Embraced the infection and turned against it's kin. They travel Hallownest seeking anything that might pose a threat to the source of their power, and hunting it down.";
            if (key == "NOTE_TRAITOR_LORD")
                return "Now they're similar to us, tiny squib... in a way.<br>You have killed<color=yellow> " + Settings.TraitorLordsKilled + " out of the 10 Traitor Lords<color=white> that roam Hallownest.";
            //All the Extra Lore Tablets
            switch (tabletNumber)
            {
                case 0:
                    if (key == "TUT_TAB_03")
                        return "The gates have been shut, all the entrances closed. If you survive the climb then the guards will surely strike you down! Whoever you are, turn around save yourself. This kingdom has nothing left to offer so don't disturb it and let it have it's final rest. " +
                               "<br><br><br>Don't let her false promises claim another soul.<br><br>" +
                               "<color=yellow>                   - One of the humble students of the Nailsage";
                break;
                case 1:
                    if (key == "TUT_TAB_03")
                        return "No matter how many times it called, it never got an answer.<br>No matter how much it ate, it never filled that emptiness.<br><br><br>" +
                               "Attempting to escape lust, it indulged itself in gluttony.";
                    break;
                case 2:
                    if (key == "TUT_TAB_03")
                        return "Some levers will not stay switched on forever.<br> These are called <color=yellow>Temporary Switches.<color=white><br>" +
                               "These levers will turn off automatically after a certain time or if you leave your shade behind.<br>" +
                               "Switching these levers can make certain platforms appear, or gates open, even across rooms.<br>In any case you'll have to be quick on your feet to make it in time.";
                    break;
            }
            return origText;
        }
     
        //-----------------------------------------------------------------------------------------------------
        //                                               Charm Changes
        //-----------------------------------------------------------------------------------------------------
        private void SaveGameAction(int id = 0) //Modify Charm Costs
        {
            PlayerData.instance.charmCost_2 = 0; //Wayward Compass costs 0 and is a passive item
            PlayerData.instance.charmCost_4 = 1; //Stalwart Shell is now Plated Shell
            PlayerData.instance.charmCost_8 = 1; //Lifeblood Heart costs 1
            PlayerData.instance.charmCost_9 = 2; //Lifeblood Core costs 2
            PlayerData.instance.charmCost_11 = 2; //Flukenest costs 2
            PlayerData.instance.charmCost_15 = 3; //Heavy Blow now costs 3 to compensate for DMG boost
            PlayerData.instance.charmCost_22 = 0; //Glowing Womb Costs 0 Charm Notch / Alternate: Cost 3 BUT the aspids are nukes
            PlayerData.instance.charmCost_26 = 0; //Nail Master's Glory costs 0 and acts like a passive buff
            PlayerData.instance.charmCost_29 = 2; //Hiveblood costs 2
            PlayerData.instance.charmCost_31 = 1; //Dashmaster costs 1
            PlayerData.instance.charmCost_34 = 3; //Deep Focus costs 3
            PlayerData.instance.charmCost_35 = 2; //Grubberfly's Elegy costs 2
            PlayerData.instance.charmCost_38 = 1; //Dreamshield costs 1
            PlayerData.instance.charmCost_39 = 1; //Weaversong costs 1
        }

        private void ChangeCharms(PlayerData pd, HeroController hc)
        {
            //Stalwart Shell now becomes "Plated Shell" and has new effects
            if (PlayerData.instance.equippedCharm_4)
            {
                Log("Plated Shell is equipped");
                platedShellEqipped = true;
                champShellDmgMultiply = -1;
            }
            else
            {
                Log("Plated Shell is unequipped");
                platedShellEqipped = false;
                champShellDmgMultiply = 2;
            }
            //FStrength halves your HP
            if (PlayerData.instance.equippedCharm_25) 
            {
                PlayerData.instance.health = (int) ((float) PlayerData.instance.GetInt("maxHealthBase") * 0.5f + 1);
                PlayerData.instance.maxHealth = (int) ((float) PlayerData.instance.GetInt("maxHealthBase") * 0.5f + 1);
            }
            //Hiveblood disables focusing when equipped
            if (PlayerData.instance.equippedCharm_29)
            {
                PlayMakerFSM sc = HeroController.instance.spellControl;
                sc.Fsm.GetFsmFloat("Time Per MP Drain UnCH").Value *= 999;
                sc.Fsm.GetFsmFloat("Time Per MP Drain CH").Value *= 999;
            }
            else
            {
                PlayMakerFSM sc = HeroController.instance.spellControl;
                sc.Fsm.GetFsmFloat("Time Per MP Drain UnCH").Value = 0.027f;
                sc.Fsm.GetFsmFloat("Time Per MP Drain CH").Value *= 0.018f;
            }
        }
        private HitInstance HitInstanceAdjust(Fsm fsm, HitInstance hit)
        {
            UpdateMoveSpeed();
            int damageDealt = hit.DamageDealt;
            //Heavy blow buffs damage by 20%
            if (hit.AttackType == AttackTypes.Nail && PlayerData.instance.equippedCharm_15) 
            {
                damageDealt = hit.DamageDealt;
                hit.DamageDealt = (int) ((double) damageDealt * 1.20);
            }
            //Spelltwister nerfs nail damage by 20%
            if (hit.AttackType == AttackTypes.Nail && PlayerData.instance.equippedCharm_33) 
            {
                damageDealt = hit.DamageDealt;
                hit.DamageDealt = (int) ((double) damageDealt * 0.8);
            }
            //Sharpshadow attack does 2.5 nail damage
            if (hit.AttackType == AttackTypes.SharpShadow) 
            {
                damageDealt = hit.DamageDealt;
                hit.DamageDealt = (int) ((double) damageDealt * 2.5);
            }

            return hit;
        }
        private void UpdateMoveSpeed()
        {
            HeroController.instance.WALK_SPEED = 6f;
            HeroController.instance.RUN_SPEED = 8.3f;
            //Sprint Master Changes
            if (PlayerData.instance.equippedCharm_37)
            {
                if (PlayerData.instance.health > 5)
                {
                    HeroController.instance.WALK_SPEED = 8f;
                    HeroController.instance.RUN_SPEED = 10.3f;
                    HeroController.instance.RUN_SPEED_CH = 10.5f;
                    HeroController.instance.RUN_SPEED_CH_COMBO = 12.5f;
                    Log("Sprintmaster speed is at 1");
                }
                else if (PlayerData.instance.health >= 3 && PlayerData.instance.health <= 5)
                {
                    HeroController.instance.WALK_SPEED = 10f;
                    HeroController.instance.RUN_SPEED = 12.3f;
                    HeroController.instance.RUN_SPEED_CH = 12.5f;
                    HeroController.instance.RUN_SPEED_CH_COMBO = 14.5f;
                    Log("Sprintmaster speed is at 2");
                }
                else if (PlayerData.instance.health < 3)
                {
                    HeroController.instance.WALK_SPEED = 12f;
                    HeroController.instance.RUN_SPEED = 14.5f;
                    HeroController.instance.RUN_SPEED_CH = 14.8f;
                    HeroController.instance.RUN_SPEED_CH_COMBO = 17.5f;
                    Log("Sprintmaster speed is at 3");
                }
            }
        }
        private int OnHealthTaken(int damage)
        {
            UpdateMoveSpeed();
            if (platedShellEqipped == true)
            {
                damage *= champShellDmgMultiply / 2;
                champShellDmgMultiply++;
            }
            if (damageMultiplierOn == true) damage *= damageMultiplierMultiply + damageMultiplierAdd;
            return damage;
        }
    }
}