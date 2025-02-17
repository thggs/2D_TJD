using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UI_game_manager : MonoBehaviour
{
    private UIDocument _doc;

    private VisualElement _gameUIWrapper;
    //private Button _buttonPause;

    [SerializeField]
    private VisualTreeAsset _pauseTemplate;
    private VisualElement _pauseMenu;
    [SerializeField]
    private VisualTreeAsset _settingsTemplate;
    private VisualElement _buttonsSettings;
    [SerializeField]
    private VisualTreeAsset _deadStats;
    private VisualElement _stats;
    [SerializeField]
    private VisualTreeAsset _upgradeButtonsTemplate;
    private VisualElement _upgradeButtons;
    [SerializeField]
    private VisualTreeAsset _highScoreInputTemplate;
    private VisualElement _highScoreInput;
    [SerializeField]
    private VisualTreeAsset _rosasTemplate;
    private VisualElement _rosas;

    private const string POPUP_ANIMATION = "pop-animation-hide";
    private int _mainPopupIndex = -1;

    private int highscore;

    private VisualElement[] _statsNames;
    private VisualElement[] _statsValues;

    private bool upgrade = true;
    public Timer timer;
    public GameStats gameStats;

    private int[] selectedInts;

    public List<Dictionary<int, string>> dictionariesList = new List<Dictionary<int, string>>();

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();

        _gameUIWrapper = _doc.rootVisualElement.Q<VisualElement>("GameUI");

        _buttonsSettings = _settingsTemplate.CloneTree();

        _pauseMenu = _pauseTemplate.CloneTree();
        var buttonResume = _pauseMenu.Q<Button>("ButtonResume");
        var buttonSettings = _pauseMenu.Q<Button>("ButtonSettings");
        var buttonBackMenu = _pauseMenu.Q<Button>("ButtonBack");

        _stats = _deadStats.CloneTree();
        //var buttonMenu = _stats.Q<Button>("MenuButton");

        _rosas = _rosasTemplate.CloneTree();

        /*_statsNames = _stats.Q<VisualElement>("Stats").Children().ToArray();
        _statsValues = _stats.Q<VisualElement>("Values").Children().ToArray();*/


        //_stats.RegisterCallback<TransitionEndEvent>(Stats_TransitionEnd);


        buttonResume.clicked += ButtonResume_clicked;
        buttonSettings.clicked += ButtonSettings_clicked;
        buttonBackMenu.clicked += ButtonLoadMenu_clicked;



        var buttonBack = _buttonsSettings.Q<Button>("ButtonBack");
        var musicVolumeSlider = _buttonsSettings.Q<Slider>("AmbientSoundSlider");
        var effectsVolumeSlider = _buttonsSettings.Q<Slider>("SoundEffectsSlider");

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVolume", 1.0f);
        }

        if (PlayerPrefs.HasKey("EffectsVolume"))
        {
            effectsVolumeSlider.value = PlayerPrefs.GetFloat("EffectsVolume");
        }
        else
        {
            PlayerPrefs.SetFloat("EffectsVolume", 1.0f);
        }

        buttonBack.clicked += ButtonBack_clicked;

        //buttonMenu.clicked += ButtonBackMenu_clicked;

        musicVolumeSlider.RegisterValueChangedCallback(v =>
        {
            var newValue = v.newValue;
            PlayerPrefs.SetFloat("MusicVolume", newValue);
        });

        effectsVolumeSlider.RegisterValueChangedCallback(v =>
        {
            var newValue = v.newValue;
            PlayerPrefs.SetFloat("EffectsVolume", newValue);
        });

        // initial stats
        /*gameStats.player.PlayerHealth = 1000;
        gameStats.player.PlayerMaxHealth = 1000;
        gameStats.player.PlayerXP = 0;
        gameStats.player.PlayerLevel = 1;
        gameStats.player.PlayerSpeed = 5;
        gameStats.player.PlayerSpeedLevel = 1;
        gameStats.player.PlayerPickupRadius = 0.5f;
        gameStats.player.PlayerMaxHealthLevel = 1;
        gameStats.player.PlayerPickupLevel = 1;

        gameStats.healingStones.HealAmount = 50;
        gameStats.healingStones.HealLevel = 1;

        gameStats.whip.WhipLevel = 1;
        gameStats.whip.WhipCooldown = 1;
        gameStats.whip.WhipDamage = 5;
        gameStats.whip.WhipProjectiles = 1;
        gameStats.whip.WhipDelay = 0.1f;

        gameStats.bible.BibleProjectiles = 0;
        gameStats.bible.BibleLevel = 0;
        gameStats.bible.BibleDamage = 5;
        gameStats.bible.BibleCooldown = 5;
        gameStats.bible.BibleLifetime = 3;

        gameStats.holyWater.WaterProjectiles = 0;
        gameStats.holyWater.WaterLevel = 0;
        gameStats.holyWater.WaterDamage = 0.1f;
        gameStats.holyWater.WaterCooldown = 8;
        gameStats.holyWater.WaterLifetime = 4;

        gameStats.throwingKnife.KnifeLevel = 0;
        gameStats.throwingKnife.KnifeCooldown = 6;
        gameStats.throwingKnife.KnifeDamage = 5;
        gameStats.throwingKnife.KnifeProjectiles = 0;*/
    }

    private void Stats_TransitionEnd(TransitionEndEvent evt)
    {
        if (!evt.stylePropertyNames.Contains("opacity")) { return; }
        if (_mainPopupIndex < _statsNames.Length - 1)
        {
            _mainPopupIndex++;
            _statsNames[_mainPopupIndex].ToggleInClassList(POPUP_ANIMATION);
        }
        if (_mainPopupIndex < _statsValues.Length - 1)
        {
            _mainPopupIndex++;
            _statsValues[_mainPopupIndex].ToggleInClassList(POPUP_ANIMATION);
        }
    }

    public void ChatBox(bool show)
    {
        if (show)
        {
            _gameUIWrapper.Clear();
            _gameUIWrapper.Add(_rosas);
        }
        else
        {
            _gameUIWrapper.Clear();
        }
    }

    private void ButtonPause_clicked()
    {
        _gameUIWrapper.Clear();
        _gameUIWrapper.Add(_pauseMenu);
        timer.stopTimer(false);
    }

    private void ButtonResume_clicked()
    {
        if (!upgrade)
        {
            ShowUpgrades();
        }
        else
        {
            _gameUIWrapper.Clear();
            timer.stopTimer(false);
            Time.timeScale = 1;
        }
    }

    private void ButtonSettings_clicked()
    {
        _gameUIWrapper.Clear();
        _gameUIWrapper.Add(_buttonsSettings);
    }

    private void ButtonBackMenu_clicked()
    {
        if (PlayerPrefs.GetInt("FourthScore") > highscore)
        {
            PlayerPrefs.SetString("FifthName", _highScoreInput.Q<TextField>("HighScoreInput").text);
        }
        else if (PlayerPrefs.GetInt("ThirdScore") > highscore)
        {
            PlayerPrefs.SetString("FifthName", PlayerPrefs.GetString("FourthName"));
            PlayerPrefs.SetString("FourthName", _highScoreInput.Q<TextField>("HighScoreInput").text);
        }
        else if (PlayerPrefs.GetInt("SecondScore") > highscore)
        {
            PlayerPrefs.SetString("FifthName", PlayerPrefs.GetString("FourthName"));
            PlayerPrefs.SetString("FourthName", PlayerPrefs.GetString("ThirdName"));
            PlayerPrefs.SetString("ThirdName", _highScoreInput.Q<TextField>("HighScoreInput").text);
        }
        else if (PlayerPrefs.GetInt("FirstScore") > highscore)
        {
            PlayerPrefs.SetString("FifthName", PlayerPrefs.GetString("FourthName"));
            PlayerPrefs.SetString("FourthName", PlayerPrefs.GetString("ThirdName"));
            PlayerPrefs.SetString("ThirdName", PlayerPrefs.GetString("SecondName"));
            PlayerPrefs.SetString("SecondName", _highScoreInput.Q<TextField>("HighScoreInput").text);
        }
        else
        {
            PlayerPrefs.SetString("FifthName", PlayerPrefs.GetString("FourthName"));
            PlayerPrefs.SetString("FourthName", PlayerPrefs.GetString("ThirdName"));
            PlayerPrefs.SetString("ThirdName", PlayerPrefs.GetString("SecondName"));
            PlayerPrefs.SetString("SecondName", PlayerPrefs.GetString("FirstName"));
            PlayerPrefs.SetString("FirstName", _highScoreInput.Q<TextField>("HighScoreInput").text);
        }
        PlayerPrefs.Save();
        SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
    }

    private void ButtonLoadMenu_clicked()
    {
        SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
    }

    private void ButtonBack_clicked()
    {
        _gameUIWrapper.Clear();
        _gameUIWrapper.Add(_pauseMenu);
    }

    private void ButtonHighScoreInput_clicked()
    {
        _highScoreInput = _highScoreInputTemplate.CloneTree();
        var highScoreName = _highScoreInput.Q<TextField>("HighScoreInput");
        var menubuttonScore = _highScoreInput.Q<Button>("MenuButton");

        menubuttonScore.clicked += ButtonBackMenu_clicked;
        _gameUIWrapper.Clear();
        _gameUIWrapper.Add(_highScoreInput);
    }

    // Start is called before the first frame update
    void Start()
    {
        Dictionaries();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStats.player.PlayerHealth > 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //this.ButtonPause_clicked();
                Time.timeScale = 0;
                _gameUIWrapper.Clear();
                _gameUIWrapper.Add(_pauseMenu);
                timer.stopTimer(true);
                //_gameUIWrapper.Add(_pauseMenu);
            }
        }

        if (gameStats.player.PlayerXP >= gameStats.player.PlayerLevel * 10)
        {
            Upgrade();
        }
    }

    public void EndGame(bool youWin)
    {
        Time.timeScale = 0;
        timer.stopTimer(true);
        _gameUIWrapper.Clear();
        _gameUIWrapper.Add(_stats);
        if (youWin)
        {
            _stats.Q<Label>("Statistics").text = "You Won!";
        }
        else
        {
            _stats.Q<Label>("Statistics").text = "Try again...";
        }
        _stats.Q<Label>("ZombiesVal").text = gameStats.enemiesKilled.zombies.ToString("0");
        _stats.Q<Label>("BatsVal").text = gameStats.enemiesKilled.bats.ToString("0");
        _stats.Q<Label>("SkeletonsVal").text = gameStats.enemiesKilled.skeletons.ToString("0");
        _stats.Q<Label>("CrawlersVal").text = gameStats.enemiesKilled.crawlers.ToString("0");
        _stats.Q<Label>("FlyingEyesVal").text = gameStats.enemiesKilled.flyingEyes.ToString("0");
        _stats.Q<Label>("WraithsVal").text = gameStats.enemiesKilled.wraiths.ToString("0");
        _stats.Q<Label>("TimeVal").text = timer.GetTime();
        _stats.Q<Label>("PlayerLevelVal").text = gameStats.player.PlayerLevel.ToString("0");
        //_stats.RegisterCallback<TransitionEndEvent>(Stats_TransitionEnd);
        //Stats_TransitionEnd(new TransitionEndEvent());
        //_stats.ToggleInClassList(POPUP_ANIMATION);
        highscore = gameStats.enemiesKilled.zombies + gameStats.enemiesKilled.bats * 2 + gameStats.enemiesKilled.skeletons * 3 + gameStats.enemiesKilled.crawlers * 4 + gameStats.enemiesKilled.flyingEyes * 4 + gameStats.enemiesKilled.wraiths * 5 + gameStats.player.PlayerLevel * 10;
        if (timer.GetTimeInSeconds() > 900)
        {
            highscore += Mathf.Max(900 - timer.GetTimeInSeconds(), 0);
        }
        if (youWin)
        {
            highscore += 1000;
        }
        _stats.Q<Label>("ScoreVal").text = highscore.ToString("0");

        var buttonMenu = _stats.Q<Button>("MenuButton");

        if (PlayerPrefs.GetInt("FifthScore") < highscore)
        {
            if (PlayerPrefs.GetInt("FourthScore") > highscore)
            {
                PlayerPrefs.SetInt("FifthScore", highscore);
            }
            else if (PlayerPrefs.GetInt("ThirdScore") > highscore)
            {
                PlayerPrefs.SetInt("FifthScore", PlayerPrefs.GetInt("FourthScore"));
                PlayerPrefs.SetInt("FourthScore", highscore);
            }
            else if (PlayerPrefs.GetInt("SecondScore") > highscore)
            {
                PlayerPrefs.SetInt("FifthScore", PlayerPrefs.GetInt("FourthScore"));
                PlayerPrefs.SetInt("FourthScore", PlayerPrefs.GetInt("ThirdScore"));
                PlayerPrefs.SetInt("ThirdScore", highscore);
            }
            else if (PlayerPrefs.GetInt("FirstScore") > highscore)
            {
                PlayerPrefs.SetInt("FifthScore", PlayerPrefs.GetInt("FourthScore"));
                PlayerPrefs.SetInt("FourthScore", PlayerPrefs.GetInt("ThirdScore"));
                PlayerPrefs.SetInt("ThirdScore", PlayerPrefs.GetInt("SecondScore"));
                PlayerPrefs.SetInt("SecondScore", highscore);
            }
            else
            {
                PlayerPrefs.SetInt("FifthScore", PlayerPrefs.GetInt("FourthScore"));
                PlayerPrefs.SetInt("FourthScore", PlayerPrefs.GetInt("ThirdScore"));
                PlayerPrefs.SetInt("ThirdScore", PlayerPrefs.GetInt("SecondScore"));
                PlayerPrefs.SetInt("SecondScore", PlayerPrefs.GetInt("FirstScore"));
                PlayerPrefs.SetInt("FirstScore", highscore);
            }
            buttonMenu.clicked += ButtonHighScoreInput_clicked;
        }
        else
        {
            buttonMenu.clicked += ButtonLoadMenu_clicked;
        }

    }

    void Upgrade()
    {
        // List of posible upgrades


        gameStats.player.PlayerLevel++;
        gameStats.player.PlayerXP = 0;
        Time.timeScale = 0;
        upgrade = false;

        List<int> upgrades = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

        // Remove from list if max level
        if (gameStats.player.PlayerMaxHealthLevel == 7)
            upgrades.Remove(0);
        if (gameStats.healingStones.HealLevel == 7)
            upgrades.Remove(1);
        if (gameStats.player.PlayerSpeedLevel == 7)
            upgrades.Remove(2);
        if (gameStats.player.PlayerPickupLevel == 7)
            upgrades.Remove(3);
        if (gameStats.whip.WhipLevel == 9)
            upgrades.Remove(4);
        if (gameStats.bible.BibleLevel == 9)
            upgrades.Remove(5);
        if (gameStats.holyWater.WaterLevel == 9)
            upgrades.Remove(6);
        if (gameStats.throwingKnife.KnifeLevel == 9)
            upgrades.Remove(7);

        // Choose three different upgrades
        selectedInts = upgrades.OrderBy(x => Random.value).Take(3).ToArray();
        ShowUpgrades();
    }
    void ShowUpgrades()
    {


        // need to add option to only put 2 or 1 buttons when upgrades.size() < 3 !!!

        // Add corresponding text to buttons
        string option1 = dictionariesList[selectedInts[0]][selectLevel(selectedInts[0])];
        string option2 = dictionariesList[selectedInts[1]][selectLevel(selectedInts[1])];
        string option3 = dictionariesList[selectedInts[2]][selectLevel(selectedInts[2])];

        _upgradeButtons = _upgradeButtonsTemplate.CloneTree();
        var level = _upgradeButtons.Q<Label>("Level");
        var upgradeButton1 = _upgradeButtons.Q<Button>("upgradeButton1");
        var upgradeButton2 = _upgradeButtons.Q<Button>("upgradeButton2");
        var upgradeButton3 = _upgradeButtons.Q<Button>("upgradeButton3");
        var imageButton1 = _upgradeButtons.Q<Label>("imageButton1");
        var imageButton2 = _upgradeButtons.Q<Label>("imageButton2");
        var imageButton3 = _upgradeButtons.Q<Label>("imageButton3");

        _gameUIWrapper.Clear();
        _gameUIWrapper.Add(_upgradeButtons);

        level.text = "Level " + gameStats.player.PlayerLevel.ToString("0");

        upgradeButton1.text = option1;
        upgradeButton2.text = option2;
        upgradeButton3.text = option3;

        // Add corresponding images to button
        StyleBackground styleBackground1 = new StyleBackground(SelectImage(selectedInts[0]));
        StyleBackground styleBackground2 = new StyleBackground(SelectImage(selectedInts[1]));
        StyleBackground styleBackground3 = new StyleBackground(SelectImage(selectedInts[2]));
        //styleBackground.texture = SelectImage(selectedInts[0]);
        imageButton1.style.backgroundImage = styleBackground1;
        imageButton2.style.backgroundImage = styleBackground2;
        imageButton3.style.backgroundImage = styleBackground3;

        upgradeButton1.clicked += upgradeButton1_clicked;
        upgradeButton2.clicked += upgradeButton2_clicked;
        upgradeButton3.clicked += upgradeButton3_clicked;

    }

    public int selectLevel(int option)
    {
        switch (option)
        {
            case 0: return (gameStats.player.PlayerMaxHealthLevel + 1);
            case 1: return (gameStats.healingStones.HealLevel + 1);
            case 2: return (gameStats.player.PlayerSpeedLevel + 1);
            case 3: return (gameStats.player.PlayerPickupLevel + 1);
            case 4: return (gameStats.whip.WhipLevel + 1);
            case 5: return (gameStats.bible.BibleLevel + 1);
            case 6: return (gameStats.holyWater.WaterLevel + 1);
            case 7: return (gameStats.throwingKnife.KnifeLevel + 1);
            default: return 0;
        }
    }

    private void upgradeButton1_clicked()
    {

        LevelUp(selectedInts[0]);
        upgrade = true;

    }

    private void upgradeButton2_clicked()
    {

        LevelUp(selectedInts[1]);
        upgrade = true;

    }

    private void upgradeButton3_clicked()
    {

        LevelUp(selectedInts[2]);
        upgrade = true;

    }

    public void LevelUp(int option)
    {
        switch (option)
        {
            // Max Health
            case 0:
                gameStats.player.PlayerMaxHealth += 100;
                gameStats.player.PlayerMaxHealthLevel += 1;
                gameStats.player.PlayerHealth += 100;
                Debug.Log("Level Up Max Health");
                break;

            // Healing Stones
            case 1:
                gameStats.healingStones.HealLevel += 1;
                gameStats.healingStones.HealAmount += 25;
                Debug.Log("Level Up Healing Stones");
                break;

            // Player Speed
            case 2:
                gameStats.player.PlayerSpeedLevel += 1;
                gameStats.player.PlayerSpeed += 0.5f;
                Debug.Log("Level Up Player Speed");
                break;

            // XP Radius
            case 3:
                gameStats.player.PlayerPickupLevel += 1;
                gameStats.player.PlayerPickupRadius += 0.5f;
                break;

            // Whip
            case 4:
                gameStats.whip.WhipLevel += 1;
                switch (gameStats.whip.WhipLevel)
                {
                    case 2: gameStats.whip.WhipDamage = 6.25f; break;
                    case 3: gameStats.whip.WhipCooldown = 0.8f; break;
                    case 4: gameStats.whip.WhipDamage = 8.0f; gameStats.whip.WhipProjectiles = 2; break;
                    case 5: gameStats.whip.WhipCooldown = 0.7f; break;
                    case 6: gameStats.whip.WhipDamage = 10.0f; break;
                    case 7: gameStats.whip.WhipCooldown = 0.6f; break;
                    case 8: gameStats.whip.WhipDamage = 12.5f; break;
                    case 9: gameStats.whip.WhipDamage = 15.0f; gameStats.whip.WhipCooldown = 0.6f; break;
                }
                Debug.Log("Level Up Whip");
                break;

            // Bible
            case 5:
                gameStats.bible.BibleLevel += 1;
                switch (gameStats.bible.BibleLevel)
                {
                    case 1: gameStats.bible.BibleProjectiles = 1; break;
                    case 2: gameStats.bible.BibleDamage = 6.25f; break;
                    case 3: gameStats.bible.BibleProjectiles = 2; gameStats.bible.BibleLifetime = 4.0f; break;
                    case 4: gameStats.bible.BibleDamage = 8.0f; break;
                    case 5: gameStats.bible.BibleProjectiles = 3; gameStats.bible.BibleCooldown = 4.0f; break;
                    case 6: gameStats.bible.BibleDamage = 10.0f; break;
                    case 7: gameStats.bible.BibleProjectiles = 4; gameStats.bible.BibleLifetime = 5.0f; break;
                    case 8: gameStats.bible.BibleDamage = 12.5f; break;
                    case 9: gameStats.bible.BibleDamage = 16.0f; break;
                }
                Debug.Log("Level Up Bible");
                break;

            // Holy Water
            case 6:
                gameStats.holyWater.WaterLevel += 1;
                switch (gameStats.holyWater.WaterLevel)
                {
                    case 1: gameStats.holyWater.WaterProjectiles = 1; break;
                    case 2: gameStats.holyWater.WaterProjectiles = 2; gameStats.holyWater.WaterDamage = 0.125f; break;
                    case 3: gameStats.holyWater.WaterDamage = 0.15f; gameStats.holyWater.WaterLifetime = 5; break;
                    case 4: gameStats.holyWater.WaterProjectiles = 3; gameStats.holyWater.WaterCooldown = 7; break;
                    case 5: gameStats.holyWater.WaterDamage = 0.2f; break;
                    case 6: gameStats.holyWater.WaterProjectiles = 4; gameStats.holyWater.WaterLifetime = 6; break;
                    case 7: gameStats.holyWater.WaterProjectiles = 5; gameStats.holyWater.WaterCooldown = 6; break;
                    case 8: gameStats.holyWater.WaterProjectiles = 6; gameStats.holyWater.WaterDamage = 0.25f; break;
                    case 9: gameStats.holyWater.WaterProjectiles = 6; gameStats.holyWater.WaterDamage = 0.3f; break;
                }
                Debug.Log("Level Up Holy Water");
                break;

            // Knife
            case 7:
                gameStats.throwingKnife.KnifeLevel += 1;
                switch (gameStats.throwingKnife.KnifeLevel)
                {
                    case 1: gameStats.throwingKnife.KnifeProjectiles = 1; break;
                    case 2: gameStats.throwingKnife.KnifeProjectiles = 2; gameStats.throwingKnife.KnifeDamage = 6.25f; break;
                    case 3: gameStats.throwingKnife.KnifeCooldown = 3.5f; gameStats.throwingKnife.KnifeDurability = 2; break;
                    case 4: gameStats.throwingKnife.KnifeProjectiles = 3; gameStats.throwingKnife.KnifeDamage = 8; break;
                    case 5: gameStats.throwingKnife.KnifeProjectiles = 4; gameStats.throwingKnife.KnifeCooldown = 3; break;
                    case 6: gameStats.throwingKnife.KnifeDamage = 10.0f; break;
                    case 7: gameStats.throwingKnife.KnifeProjectiles = 5; gameStats.throwingKnife.KnifeDamage = 12.5f; break;
                    case 8: gameStats.throwingKnife.KnifeCooldown = 2.5f; gameStats.throwingKnife.KnifeDurability = 3; break;
                    case 9: gameStats.throwingKnife.KnifeProjectiles = 6; gameStats.throwingKnife.KnifeDamage = 16.0f; break;
                }
                Debug.Log("Level Up Knife");
                break;
        }
        // Remove level up panel and resume game
        Time.timeScale = 1;
        _gameUIWrapper.Clear();
    }

    public Sprite SelectImage(int option)
    {
        switch (option)
        {
            case 0: Sprite maxHealthImage = Resources.Load<Sprite>("Images/maxHealth"); return maxHealthImage;
            case 1: Sprite healthImage = Resources.Load<Sprite>("Images/health"); return healthImage;
            case 2: Sprite speedImage = Resources.Load<Sprite>("Images/speed"); return speedImage;
            case 3: Sprite xpRadiusImage = Resources.Load<Sprite>("Images/xpRadius"); return xpRadiusImage;
            case 4: Sprite whipSprite = Resources.Load<Sprite>("Images/whip"); return whipSprite;
            case 5: Sprite bibleSprite = Resources.Load<Sprite>("Images/bible"); return bibleSprite;
            case 6: Sprite holyWaterSprite = Resources.Load<Sprite>("Images/holyWater"); return holyWaterSprite;
            case 7: Sprite knifeSprite = Resources.Load<Sprite>("Images/knife"); return knifeSprite;
            default: Sprite maxHealthImagea = Resources.Load<Sprite>("Images/maxHealth"); return maxHealthImagea;
        }
    }


    public void Dictionaries()
    {
        // Create dictionaries
        Dictionary<int, string> maxHealthDictionary = new Dictionary<int, string>();
        Dictionary<int, string> healthPickupsDictionary = new Dictionary<int, string>();
        Dictionary<int, string> speedDictionary = new Dictionary<int, string>();
        Dictionary<int, string> xpRadiusDictionary = new Dictionary<int, string>();
        Dictionary<int, string> whipDictionary = new Dictionary<int, string>();
        Dictionary<int, string> bibleDictionary = new Dictionary<int, string>();
        Dictionary<int, string> holyWaterDictionary = new Dictionary<int, string>();
        Dictionary<int, string> knifeDictionary = new Dictionary<int, string>();

        // Assign Values - Player
        // Max Health -> 0
        maxHealthDictionary.Add(2, "<b>Level 2:</b> Increase Max Health by 10%");
        maxHealthDictionary.Add(3, "<b>Level 3:</b> Increase Max Health by 10%");
        maxHealthDictionary.Add(4, "<b>Level 4:</b> Increase Max Health by 10%");
        maxHealthDictionary.Add(5, "<b>Level 5:</b> Increase Max Health by 10%");
        maxHealthDictionary.Add(6, "<b>Level 6:</b> Increase Max Health by 10%");
        maxHealthDictionary.Add(7, "<b>Level 7:</b> Increase Max Health by 10%");

        // Health Pickups -> 1
        healthPickupsDictionary.Add(2, "<b>Level 2:</b> Increase health restored by Pickups 50%");
        healthPickupsDictionary.Add(3, "<b>Level 3:</b> Increase health restored by Pickups 50%");
        healthPickupsDictionary.Add(4, "<b>Level 4:</b> Increase health restored by Pickups 50%");
        healthPickupsDictionary.Add(5, "<b>Level 5:</b> Increase health restored by Pickups 50%");
        healthPickupsDictionary.Add(6, "<b>Level 6:</b> Increase health restored by Pickups 50%");
        healthPickupsDictionary.Add(7, "<b>Level 7:</b> Increase health restored by Pickups 50%");

        // Player Speed -> 2
        speedDictionary.Add(2, "<b>Level 2:</b> Increase Player Speed 10%");
        speedDictionary.Add(3, "<b>Level 3:</b> Increase Player Speed 10%");
        speedDictionary.Add(4, "<b>Level 4:</b> Increase Player Speed 10%");
        speedDictionary.Add(5, "<b>Level 5:</b> Increase Player Speed 10%");
        speedDictionary.Add(6, "<b>Level 6:</b> Increase Player Speed 10%");
        speedDictionary.Add(7, "<b>Level 7:</b> Increase Player Speed 10%");

        // XP Radius -> 3
        xpRadiusDictionary.Add(2, "<b>Level 2:</b> Increase XP Radius to 1");
        xpRadiusDictionary.Add(3, "<b>Level 3:</b> Increase XP Radius to 1.5");
        xpRadiusDictionary.Add(4, "<b>Level 4:</b> Increase XP Radius to 2");
        xpRadiusDictionary.Add(5, "<b>Level 5:</b> Increase XP Radius to 2.5");
        xpRadiusDictionary.Add(6, "<b>Level 6:</b> Increase XP Radius to 3");
        xpRadiusDictionary.Add(7, "<b>Level 7:</b> Increase XP Radius to 3.5");

        // Assign Values - Weapons
        // Whip -> 4
        whipDictionary.Add(2, "<b>Level 2:</b> Increase Whip Damage by 25%");
        whipDictionary.Add(3, "<b>Level 3:</b> Reduce Whip Cooldown by 20%");
        whipDictionary.Add(4, "<b>Level 4:</b> Increase Whip Projectiles to 2 and Damage by 25%");
        whipDictionary.Add(5, "<b>Level 5:</b> Reduce Whip Cooldown by 10%");
        whipDictionary.Add(6, "<b>Level 6:</b> Increase Whip Damage by 25%");
        whipDictionary.Add(7, "<b>Level 7:</b> Reduce Whip Cooldown by 10%");
        whipDictionary.Add(8, "<b>Level 8:</b> Increase Whip Damage by 25%");
        whipDictionary.Add(9, "<b>Level 9:</b> Increase Whip Damage by 25% and Reduce Whip Cooldown by 10%");

        // Bible -> 5
        bibleDictionary.Add(1, "<b>Level 1:</b> Unlock Bible Weapon");
        bibleDictionary.Add(2, "<b>Level 2:</b> Increase Bible Damage by 25%");
        bibleDictionary.Add(3, "<b>Level 3:</b> Increase Amount of Bibles to 2 and Lifetime by 1s");
        bibleDictionary.Add(4, "<b>Level 4:</b> Increase Bible Damage by 25%");
        bibleDictionary.Add(5, "<b>Level 5:</b> Increase Amount of Bibles to 3 and Reduce Cooldown by 1s");
        bibleDictionary.Add(6, "<b>Level 6:</b> Increase Bible Damage by 25%");
        bibleDictionary.Add(7, "<b>Level 7:</b> Increase Amount of Bibles to 4 and Lifetime by 1s");
        bibleDictionary.Add(8, "<b>Level 8:</b> Increase Bible Damage by 25%");
        bibleDictionary.Add(9, "<b>Level 9:</b> Increase Bible Damage by 25%");

        // Holy Water -> 6
        holyWaterDictionary.Add(1, "<b>Level 1:</b> Unlock Holy Water Weapon");
        holyWaterDictionary.Add(2, "<b>Level 2:</b> Increase Amount of Holy Waters to 2 and Damage by 25%");
        holyWaterDictionary.Add(3, "<b>Level 3:</b> Increase Holy Water Damage by 25% and Lifetime by 1s");
        holyWaterDictionary.Add(4, "<b>Level 4:</b> Increase Amount of Holy Waters to 3 and Reduce Cooldown by 1s");
        holyWaterDictionary.Add(5, "<b>Level 5:</b> Increase Holy Water Damage by 25%");
        holyWaterDictionary.Add(6, "<b>Level 6:</b> Increase Amount of Holy Waters to 4 and Lifetime by 1s");
        holyWaterDictionary.Add(7, "<b>Level 7:</b> Increase Amount of Holy Waters to 5 and Reduce Cooldown by 1s");
        holyWaterDictionary.Add(8, "<b>Level 8:</b> Increase Amount of Holy Waters to 6 and Damage by 25%");
        holyWaterDictionary.Add(9, "<b>Level 9:</b> Increase Amount of Holy Waters to 7 and Damage by 25%");

        // Knife -> 7
        knifeDictionary.Add(1, "<b>Level 1:</b> Unlock Throwing Knifes Weapon");
        knifeDictionary.Add(2, "<b>Level 2:</b> Increase Amount of Knifes to 2 and Damage by 25%");
        knifeDictionary.Add(3, "<b>Level 3:</b> Reduce Knifes Cooldown by 1s and Increase Durability to 2");
        knifeDictionary.Add(4, "<b>Level 4:</b> Increase Amount of Knifes to 3 and Damage by 25%");
        knifeDictionary.Add(5, "<b>Level 5:</b> Increase Amount of Knifes to 4 Reduce Cooldown by 1s");
        knifeDictionary.Add(6, "<b>Level 6:</b> Increase Knifes Damage by 25%");
        knifeDictionary.Add(7, "<b>Level 7:</b> Increase Amount of Knifes to 5 and Damage by 25%");
        knifeDictionary.Add(8, "<b>Level 8:</b> Reduce Knifes Cooldown by 1s and Increase Durability to 3");
        knifeDictionary.Add(9, "<b>Level 9:</b> Increase Amount of Knifes to 6 and Damage by 25%");

        // Add Dictionaries to list of dictionaries
        dictionariesList.Add(maxHealthDictionary);              //  -> 0
        dictionariesList.Add(healthPickupsDictionary);          //  -> 1
        dictionariesList.Add(speedDictionary);                  //  -> 2
        dictionariesList.Add(xpRadiusDictionary);               //  -> 3
        dictionariesList.Add(whipDictionary);                   //  -> 4
        dictionariesList.Add(bibleDictionary);                  //  -> 5
        dictionariesList.Add(holyWaterDictionary);              //  -> 6
        dictionariesList.Add(knifeDictionary);                  //  -> 7
    }
}
