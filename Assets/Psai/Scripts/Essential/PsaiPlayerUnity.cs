//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using psai.net;


public class PsaiPlayerUnity : MonoBehaviour 
{
    public bool _showIntensityLevels = true;
    public bool _showIntensityControls = true;
    public bool _showThemeTriggerSection = true;
    public bool _showControlButtonSection = true;
    public bool _showListSections = true;
    public bool _showMainVolumeSlider = true;


    static readonly float GUI_FLASH_FREQUENCY = 1.0f;
    static readonly Color COLOR_CURRENT_THEME   = new Color(0.2f, 1.0f, 0.2f);
    static readonly Color COLOR_UPCOMING_THEME  = new Color(1.0f, 1.0f, 0);
    static readonly Color COLOR_LIGHTGREY = new Color(0.85f, 0.85f, 0.85f);
    static readonly Color COLOR_DARKGREY = new Color(0.40f, 0.40f, 0.40f);
    static readonly Color COLOR_DARKGREEN = new Color(0, 0.4f, 0, 1.0f);
    static readonly Color COLOR_LIGHTGREEN = new Color(0, 1.0f, 0, 1.0f);
    static readonly Color COLOR_LIGHTRED = new Color(1.0f, 0.2f, 0.2f);

    static readonly int FONTSIZE = 12;
    static GUIStyle guiStyle = new GUIStyle();

    static readonly float FORCE_SEGMENT_INFO_UPDATE_INTERVAL_IN_SECONDS = 1.0f;

    static readonly string[] _columnNamesSegments = { "Name", "Suitabilities", "Intensity", "Playcount" };
    static readonly int[] _columnWidthsRatiosSegments = { 2, 4, 8, 8 };
    static readonly string[] _columnNamesThemeList = { "Name", "id", "ThemeType" };
    static readonly int[] _columnWidthsRatiosThemeList = { 2, 4, 4 };

    private float _deltaIntensityValue = 0.05f;
    private string _deltaIntensityString = "";

    public Texture2D _textureIntensity;
    public Texture2D _textureWhiteAlpha;
    public Texture2D _textureWhiteFull;
    public Texture2D _textureNeedMusic;

    bool _flashIncrease;

    String _lastErrorString;

    int[] _themeIds;
    List<int> _themeIdsList;

    bool _sortThemesAscending;
    bool _sortSegmentsAscending;

    bool _highlightTriggered;

    int _selectedThemeId = -1;
    Dictionary<int, ThemeInfo> _themeInfos = new Dictionary<int, ThemeInfo>();
    Dictionary<ThemeType, List<int>> _themeTypesToThemeIds = new Dictionary<ThemeType, List<int>>();
    Dictionary<int, SegmentInfo> _segmentInfos = new Dictionary<int, SegmentInfo>();
    List<int> _segmentIdsListOfSelectedTheme = new List<int>();
    Dictionary<int, int> _playbackCountdowns = new Dictionary<int, int>();

    float _intensity = 1.0f;
    float _volume = 1.0f;

    bool    _configureMenuMode      = false;
    int     _menuThemeId            = -1;
    int     _menuThemeIdOld = -1;
    int     _menuThemeIndex         = 0;
    float   _menuThemeIntensity     = 1.0f;
    float   _menuThemeIntensityOld  = 1.0f;

    bool    _configureCutScene      = false;
    int     _cutSceneThemeId        = -1;
    int     _cutSceneThemeIdOld = -1;
    int     _cutSceneThemeIndex = 0;
    float   _cutSceneThemeIntensity = 1.0f;
    float   _cutSceneThemeIntensityOld = 1.0f;

    bool _showListView = false;
    bool _switchedToListViewInCurrentFrame = false;
    bool _autoScrollToCurrentSegment = true;

    Vector2 _scrollPositionThemeList        = Vector2.zero;
    Vector2 _scrollPositionSegmenttList      = Vector2.zero;
    Vector2 _scrollPositionControlBoxes     = Vector2.zero;
    Vector2 _scrollPositionTriggerSection   = Vector2.zero;

    int _playingSegmentIdInLastFrame;
    int _playingThemeIdInLastFrame;
    int _selectedThemeIdInLastFrame;

    float _flashIntensity;
    bool _paused;

    bool _initialized = false;    
    float timerForceSegmentInfoUpdateCounter = 0;


    void Awake()
    {
        guiStyle.fontSize = FONTSIZE;
        guiStyle.normal.textColor = Color.white;
        guiStyle.hover.textColor = Color.white;
    }

	void BuildDatastructuresBasedOnCurrentPsaiSoundtrack() 
	{			
        SoundtrackInfo soundtrackInfo = PsaiCore.Instance.GetSoundtrackInfo();

        if (soundtrackInfo.themeCount > 0)
        {
            _themeIds = soundtrackInfo.themeIds;
            _themeIdsList = new List<int>();
            _themeIdsList.AddRange(_themeIds);

            _themeTypesToThemeIds[ThemeType.basicMood] = new List<int>();
            _themeTypesToThemeIds[ThemeType.basicMoodAlt] = new List<int>();
            _themeTypesToThemeIds[ThemeType.dramaticEvent] = new List<int>();
            _themeTypesToThemeIds[ThemeType.action] = new List<int>();
            _themeTypesToThemeIds[ThemeType.shock] = new List<int>();
            _themeTypesToThemeIds[ThemeType.highlightLayer] = new List<int>();

            // Build _themeInfos Cache
            foreach (int themeId in _themeIds)
            {
                ThemeInfo themeInfo = PsaiCore.Instance.GetThemeInfo(themeId);
                _themeInfos[themeId] = themeInfo;

                if (_themeTypesToThemeIds.ContainsKey(themeInfo.type))
                {
                    _themeTypesToThemeIds[themeInfo.type].Add(themeId);
                }
            }

            RebuildSegmentInfoCache();


            if (_themeIds.Length > 1)
            {
                _cutSceneThemeIndex = 0;
                _menuThemeIndex = 1;
            }
            else
            {
                _cutSceneThemeIndex = 0;
                _menuThemeIndex = 0;
            }

            _menuThemeId = _themeIds[_menuThemeIndex];
            _cutSceneThemeId = _themeIds[_cutSceneThemeIndex];

            _flashIntensity = 0.0f;
            _flashIncrease = true;

            _initialized = true;
        }

        _deltaIntensityString = _deltaIntensityValue.ToString("F2");
	}

    void Update()
    {
        if (!this._initialized)
        {
            BuildDatastructuresBasedOnCurrentPsaiSoundtrack();
        }

        //////////////////////////////////////////////////////////////////////////
        // cycle GUI color flashing
        //////////////////////////////////////////////////////////////////////////
        float flashDeltaIntensity = Time.deltaTime * GUI_FLASH_FREQUENCY;

        if (_flashIncrease)
        {
            _flashIntensity += flashDeltaIntensity;
            if (_flashIntensity > 1.0f)
            {
                _flashIntensity = 2.0f - _flashIntensity;
                _flashIncrease = false;
            }
        }
        else
        {
            _flashIntensity -= flashDeltaIntensity;
            if (_flashIntensity < 0.0f)
            {
                _flashIntensity = -_flashIntensity;
                _flashIncrease = true;
            }
        }
        //////////////////////////////////////////////////////////////////////////
       
        string lastError = PsaiCore.Instance.GetLastError();

        if (lastError != null)
            _lastErrorString = lastError;
    }


    void SetGuiColorForBox(bool boxIsActive)
    {
        if (boxIsActive)
        {
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.gray;
        }
    }

    private int CompareThemesByName(int themeId1, int themeId2)
    {
        if (_themeInfos.ContainsKey(themeId1) && _themeInfos.ContainsKey(themeId2))
        {
            string themeName1 = _themeInfos[themeId1].name;
            string themeName2 = _themeInfos[themeId2].name;

            return themeName1.CompareTo(themeName2);
        }

        return 0;
    }

    private int CompareThemesByType(int themeId1, int themeId2)
    {
        if (_themeInfos.ContainsKey(themeId1) && _themeInfos.ContainsKey(themeId2))
        {
            ThemeType type1 = _themeInfos[themeId1].type;
            ThemeType type2 = _themeInfos[themeId2].type;

            return type1.CompareTo(type2);
        }

        return 0;
    }

    private int CompareSegmentsByName(int segmentId1, int segmentId2)
    {        
        if (_segmentInfos.ContainsKey(segmentId1) && _segmentInfos.ContainsKey(segmentId2))
        {         
            return _segmentInfos[segmentId1].name.CompareTo(_segmentInfos[segmentId2].name);
        }

        return 0;
    }

    private int CompareSegmentsBySuitablilies(int segmentId1, int segmentId2)
    {
        if (_segmentInfos.ContainsKey(segmentId1) && _segmentInfos.ContainsKey(segmentId2))
        {            
            string strSuitabilities1 = Segment.GetStringFromSegmentSuitabilities(_segmentInfos[segmentId1].segmentSuitabilitiesBitfield);
            string strSuitabilities2 = Segment.GetStringFromSegmentSuitabilities(_segmentInfos[segmentId2].segmentSuitabilitiesBitfield);
            return strSuitabilities1.CompareTo(strSuitabilities2);
        }

        return 0;
    }


    private int CompareSegmentsByIntensity(int snippetId1, int snippetId2)
    {
        if (_segmentInfos.ContainsKey(snippetId1) && _segmentInfos.ContainsKey(snippetId2))
        {
            return _segmentInfos[snippetId1].intensity.CompareTo(_segmentInfos[snippetId2].intensity);
        }

        return 0;
    }


    private int CompareSegmentsByPlaycount(int segmentId1, int segmentId2)
    {
        if (_segmentInfos.ContainsKey(segmentId1) && _segmentInfos.ContainsKey(segmentId2))
        {
            return _segmentInfos[segmentId1].playcount.CompareTo(_segmentInfos[segmentId2].playcount);
        }
        return 0;
    }



    void OnGUI()
    {
        GUI.skin.button.fontStyle = FontStyle.Normal;

        GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
        myButtonStyle.fontSize = FONTSIZE;

        GUIStyle myBoxStyle = new GUIStyle(GUI.skin.box);
        myBoxStyle.fontSize = FONTSIZE;

        int buttonWidth = 155;
        int buttonHeight = 25;

        int buttonWidthHalf = buttonWidth / 2;

        int buttonHeightTriggerTheme = 25;
        int intensitySliderOffsetX = 170;

        int spacingX = 15;
        int spacingY = 25;
        int spacingYsmall = 5;

        int xPos = spacingX;
        int yPos = 10;
        
        int yPosOld;

        PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
        int currentThemeId = PsaiCore.Instance.GetCurrentThemeId();
        int currentSegmentId = PsaiCore.Instance.GetCurrentSegmentId();
        int upcomingThemeId = psaiInfo.upcomingThemeId;
        bool currentSegmentChangedInThisFrame = (currentSegmentId != _playingSegmentIdInLastFrame);
        bool currentThemeChangedInThisFrame = (currentThemeId != _playingThemeIdInLastFrame);
        bool forceSegmentInfoUpdateInThisFrame = false;
        SegmentInfo currentSegmentInfo = new SegmentInfo();
        

        timerForceSegmentInfoUpdateCounter += Time.deltaTime;
        if (timerForceSegmentInfoUpdateCounter > FORCE_SEGMENT_INFO_UPDATE_INTERVAL_IN_SECONDS)
        {
            timerForceSegmentInfoUpdateCounter -= FORCE_SEGMENT_INFO_UPDATE_INTERVAL_IN_SECONDS;
            forceSegmentInfoUpdateInThisFrame = true;
        }
        
        if (_segmentInfos.ContainsKey(currentSegmentId))
        {
            if (currentSegmentChangedInThisFrame || forceSegmentInfoUpdateInThisFrame)
            {
                _segmentInfos[currentSegmentId] = PsaiCore.Instance.GetSegmentInfo(currentSegmentId);
            }
            currentSegmentInfo = _segmentInfos[currentSegmentId];
        }

        Color oldColor = GUI.backgroundColor;

        if (psaiInfo.psaiState == PsaiState.notready)
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(xPos, yPos, Screen.width, 200), "NO SOUNDTRACK LOADED\n Troubleshooting:\n1. The folder containing all psai soundtrack data must be located within the 'Resources' folder of your project.\n2. Your Scene must contain the 'Psai.prefab' Game Object with both a PsaiSoundtrackLoader and a PsaiCoreManager-Component.\n3. The PsaiSoundtrackLoader-Component needs to hold the path to the soundtrack file. Drag & Drop that file from your Soundtrack folder in the Project window.", guiStyle);
            return;
        }

        yPos = spacingYsmall;
        xPos = spacingX;
        GUI.color = Color.white;

        int intensitySliderWidth = Screen.width / 5;
        int intensitySliderHeight = spacingY;

        xPos = intensitySliderOffsetX;

        //////////////////////////////////////////////////////////////////////////
        // intensity indicator bar
        //////////////////////////////////////////////////////////////////////////   

        int intensityIndicatorHeight = (int)(spacingY * 2.0f / 2.5f);

        if (_showIntensityLevels)
        {
            xPos = intensitySliderOffsetX;

            _textureIntensity.wrapMode = TextureWrapMode.Repeat;
            _textureWhiteAlpha.wrapMode = TextureWrapMode.Repeat;

            GUI.Box(new Rect(xPos, yPos, intensitySliderWidth, intensityIndicatorHeight * 2), "", myBoxStyle);

            if (psaiInfo.psaiState == PsaiState.playing)
            {
                float intensityOfCurrentSegment = currentSegmentInfo.intensity;
                float currentIntensity = PsaiCore.Instance.GetCurrentIntensity();
                float upcomingIntensity = psaiInfo.upcomingIntensity;
                float intensityOfCurrentSegmentIndicatorWidth = intensitySliderWidth * intensityOfCurrentSegment;

                float dynamicIntensityBarHeight = intensityIndicatorHeight; // *3 / 4;
                float dynamicIntensityBarOffsetY = intensityIndicatorHeight; // intensitySliderHeight / 8;

                GUI.color = COLOR_DARKGREEN;
                GUI.DrawTextureWithTexCoords(new Rect(xPos, yPos, intensityOfCurrentSegmentIndicatorWidth, intensityIndicatorHeight), _textureIntensity, new Rect(0, 0, 1, 1));

                // non-interrupting Trigger has been received: show upcoming intensity
                if (Mathf.Abs(upcomingIntensity - currentIntensity) > 0.01f)
                {
                    GUI.color = new Color(0.5f + 0.5f * _flashIntensity, 0.5f + 0.5f * _flashIntensity, 0, 0.66f);
                    GUI.DrawTextureWithTexCoords(new Rect(xPos, yPos + dynamicIntensityBarOffsetY, (intensitySliderWidth * upcomingIntensity), dynamicIntensityBarHeight), _textureIntensity, new Rect(0, 0, 1, 1));
                }
                else
                {
                    GUI.color = COLOR_LIGHTGREEN;
                    GUI.DrawTextureWithTexCoords(new Rect(xPos, yPos + dynamicIntensityBarOffsetY, intensitySliderWidth * currentIntensity, dynamicIntensityBarHeight), _textureIntensity, new Rect(0, 0, 1, 1));
                }
            }

            xPos = spacingX;

            GUI.color = COLOR_LIGHTGREY;
            GUI.Label(new Rect(xPos, yPos - spacingYsmall, 300, spacingY), "Segment Intensity:", guiStyle);

            GUI.color = COLOR_LIGHTGREY;
            GUI.Label(new Rect(xPos, yPos - spacingYsmall + intensityIndicatorHeight, 300, spacingY), "Dynamic Intensity:", guiStyle);
        }



        //////////////////////////////////////////////////////////////////////////
        // Add To Current Intensity Button
        //////////////////////////////////////////////////////////////////////////

        if (_showIntensityControls)
        {
            xPos = intensitySliderOffsetX + intensitySliderWidth + spacingX;

            if (psaiInfo.psaiState == PsaiState.playing)
            {
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = COLOR_LIGHTGREY;
            }
        }


        //////////////////////////////////////////////////////////////////////////
        // Hold Intensity Button
        //////////////////////////////////////////////////////////////////////////

        if (_showIntensityControls)
        {
            bool holdButtonEnabled = psaiInfo.psaiState == PsaiState.playing && !PsaiCore.Instance.CutSceneIsActive() && !PsaiCore.Instance.MenuModeIsActive();

            if (psaiInfo.intensityIsHeld)
            {
                GUI.color = COLOR_CURRENT_THEME;
                GUI.DrawTextureWithTexCoords(new Rect(xPos, yPos, buttonWidthHalf, intensitySliderHeight + intensityIndicatorHeight * 2), _textureWhiteFull, new Rect(0, 0, 1, 1));
            }
            if (holdButtonEnabled)
            {
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = COLOR_DARKGREY;
            }
            if (GUI.Button(new Rect(xPos, yPos, buttonWidthHalf, intensitySliderHeight + intensityIndicatorHeight * 2), "Hold\nIntensity", myButtonStyle) && holdButtonEnabled)
            {
                PsaiCore.Instance.HoldCurrentIntensity(!psaiInfo.intensityIsHeld);
            }

            xPos += buttonWidthHalf + spacingX;

            //////////////////////////////////////////////////////////////////////////
            // Add to Intensity Button
            //////////////////////////////////////////////////////////////////////////
            if (GUI.Button(new Rect(xPos, yPos, buttonWidthHalf, intensitySliderHeight + intensityIndicatorHeight * 2), "add to\nIntensity", myButtonStyle))
            {
                if (_deltaIntensityValue > 1.0f)
                    _deltaIntensityValue = 1.0f;

                if (_deltaIntensityValue < -1.0f)
                    _deltaIntensityValue = -1.0f;

                _deltaIntensityString = _deltaIntensityValue.ToString("F2");

                PsaiCore.Instance.AddToCurrentIntensity(_deltaIntensityValue);
            }

            xPos += buttonWidthHalf + spacingX / 2;

            GUI.color = COLOR_LIGHTGREY;
            _deltaIntensityString = GUI.TextField(new Rect(xPos, yPos + intensitySliderHeight, buttonWidth / 4, intensityIndicatorHeight * 2), _deltaIntensityString, 25);
            if (!float.TryParse(_deltaIntensityString, out _deltaIntensityValue))
            {
                _deltaIntensityString = _deltaIntensityValue.ToString("F2");
            }
        }

        yPos += intensityIndicatorHeight * 2;
        yPos += intensitySliderHeight;

        //////////////////////////////////////////////////////////////////////////
        // Volume Slider
        //////////////////////////////////////////////////////////////////////////

        if (_showMainVolumeSlider)
        {
            int volumeSliderWidth = spacingX * 2;

            xPos = Screen.width - volumeSliderWidth;

            //xPos += buttonWidthHalf + spacingX;

            int volumeSliderHeight = yPos - spacingYsmall;

            GUI.color = Color.white;

            _volume = GUI.VerticalSlider(new Rect(xPos, yPos - volumeSliderHeight, volumeSliderWidth, volumeSliderHeight), PsaiCore.Instance.GetVolume(), 1.0f, 0.0f);
            {
                PsaiCore.Instance.SetVolume(_volume);
            }
            //xPos += volumeSliderWidth;
            GUI.Label(new Rect(xPos - volumeSliderWidth * 2, spacingYsmall, 200, spacingY), "volume", guiStyle);
            GUI.Label(new Rect(xPos - volumeSliderWidth * 2, spacingY, 200, spacingY), _volume.ToString("F3"), guiStyle);
        }


        //////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////
        // Trigger intensity slider
        //////////////////////////////////////////////////////////////////////////

        int themeTriggerSectionTotalHeight = _themeTypesToThemeIds.Keys.Count * (buttonHeightTriggerTheme + spacingYsmall);

        if (_showThemeTriggerSection)
        {
            yPos -= spacingY;
            xPos = spacingX;
            GUI.color = Color.white;
            GUI.Label(new Rect(xPos, yPos, 200, spacingY), "set Start-Intensity: " + _intensity.ToString("f3"), guiStyle);
            yPos += spacingYsmall * 2;

            xPos = intensitySliderOffsetX;
            float intensitySliderValue = GUI.HorizontalSlider(new Rect(xPos, yPos, intensitySliderWidth, intensitySliderHeight), _intensity, 0.0f, 1.0f);
            {
                _intensity = intensitySliderValue;
            }
        
            //////////////////////////////////////////////////////////////////////////

            yPos += spacingY;
            yPos += spacingYsmall;

            //////////////////////////////////////////////////////////////////////////
            // Theme Trigger Buttons
            //////////////////////////////////////////////////////////////////////////

            int maxThemeCountPerType = 0;
            yPosOld = yPos;
            foreach (ThemeType themeType in _themeTypesToThemeIds.Keys)
            {
                xPos = spacingX;

                if (_showThemeTriggerSection)
                {
                    GUI.color = COLOR_LIGHTGREY;
                    GUI.Label(new Rect(xPos, yPos, intensitySliderOffsetX, 50), Theme.ThemeTypeToString(themeType) + " :", guiStyle);
                }

                List<int> themeIdList = _themeTypesToThemeIds[themeType];
                if (maxThemeCountPerType < themeIdList.Count)
                {
                    maxThemeCountPerType = themeIdList.Count;
                }

                yPos += buttonHeight + spacingYsmall;
            }

            int themeTriggerButtonSectionWidth = maxThemeCountPerType * (buttonWidth + spacingX);        

            _scrollPositionTriggerSection = GUI.BeginScrollView(new Rect(intensitySliderOffsetX, yPosOld, Screen.width - intensitySliderOffsetX, themeTriggerSectionTotalHeight), _scrollPositionTriggerSection, new Rect(0, 0, themeTriggerButtonSectionWidth, themeTriggerSectionTotalHeight));

            yPos = 0;

            bool triggeringIsPossible = !PsaiCore.Instance.CutSceneIsActive() && !PsaiCore.Instance.MenuModeIsActive();
            SetGuiColorForBox(triggeringIsPossible);

            foreach (ThemeType themeType in _themeTypesToThemeIds.Keys)
            {
                xPos = 0;
                List<int> themeIdList = _themeTypesToThemeIds[themeType];

                foreach (int themeId in themeIdList)
                {                   
                    ThemeInfo themeInfo = _themeInfos[themeId];
                    if (themeInfo.type == ThemeType.highlightLayer 
                        && currentSegmentId != -1
                        && PsaiCore.Instance.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(currentSegmentId, themeInfo.id) == false)
                    {
                        GUI.enabled = false;
                    }
                    
                    bool drawButtonBackground = false;

                    if (themeId == psaiInfo.lastBasicMoodThemeId)
                    {
                        GUI.color = new Color(0.0f, 1.0f, 0.0f, 0.3f);
                        drawButtonBackground = true;
                    }

                    if (themeId == upcomingThemeId)
                    {
                        Color flashColor = new Color(1.0f, 1.0f, 0.0f, _flashIntensity);
                        GUI.color = flashColor;
                        drawButtonBackground = true;
                    }

                    if (themeId == currentThemeId)
                    {
                        switch (psaiInfo.psaiState)
                        {
                            case PsaiState.playing:
                                GUI.color = new Color(0.1f, 1.0f, 0.1f, 1.0f);
                                break;
                            case PsaiState.rest:
                                GUI.color = new Color(0.0f, 0.5f, 0.0f, _flashIntensity);
                                break;
                        }
                        drawButtonBackground = true;
                    }

                    if (drawButtonBackground)
                    {
                        GUI.DrawTextureWithTexCoords(new Rect(xPos, yPos, buttonWidth, buttonHeightTriggerTheme), _textureWhiteFull, new Rect(0, 0, 1, 1));
                        GUI.color = Color.white;
                    }

                    GUI.color = Color.white;

                    if (GUI.Button(new Rect(xPos, yPos, buttonWidth, buttonHeightTriggerTheme), themeInfo.name, myButtonStyle))
                    {
                        PsaiCore.Instance.TriggerMusicTheme(themeInfo.id, intensitySliderValue);

                        if (themeInfo.type == ThemeType.highlightLayer)
                        {
                            _highlightTriggered = true;
                        }
                    }

                    GUI.contentColor = oldColor;
                    GUI.backgroundColor = oldColor;

                    xPos += buttonWidth + spacingX;

                    GUI.enabled = true;
                }

                yPos += buttonHeight + spacingYsmall;
            }


            GUI.EndScrollView();

            yPos = yPosOld;
        }
        else
        {
            yPos += spacingYsmall * 3;
        }

        yPos += themeTriggerSectionTotalHeight;


        xPos = spacingX;
        yPos += spacingYsmall;


        //////////////////////////////////////////////////////////////////////////
        // Control Button Boxes
        //////////////////////////////////////////////////////////////////////////        

        int boxHeightStandard = (buttonHeight + spacingY) * 2;
        int boxHeightSettings = boxHeightStandard + spacingYsmall * 2;
        int boxWidth = buttonWidth + spacingX * 2;
        int boxSectionWidthTotal = boxWidth * 5 + spacingX * 4;
        int boxHeightWithScrollbar = boxHeightSettings + spacingYsmall + 5;


        if (_showControlButtonSection)
        {
            _scrollPositionControlBoxes = GUI.BeginScrollView(new Rect(0, yPos, Screen.width, boxHeightWithScrollbar), _scrollPositionControlBoxes, new Rect(0, 0, boxSectionWidthTotal, boxHeightStandard));

            xPos = Math.Max(0, (int)(((Screen.width - boxSectionWidthTotal) / 2.0f) - spacingX / 2.0f));

            yPosOld = yPos;
            yPos = 0;

            GUI.BeginGroup(new Rect(xPos, yPos, boxWidth, boxHeightStandard));
            {
                int x = spacingX;
                int y = 0;

                bool boxIsActive = !_paused && (psaiInfo.psaiState == PsaiState.playing || psaiInfo.psaiState == PsaiState.rest) && !PsaiCore.Instance.CutSceneIsActive() && !PsaiCore.Instance.MenuModeIsActive();
                SetGuiColorForBox(boxIsActive);

                GUI.Box(new Rect(0, 0, boxWidth, boxHeightStandard), "Stop Music", myBoxStyle);

                if (boxIsActive)
                {
                    y += buttonHeight;
                    if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "immediately", myButtonStyle))
                    {
                        PsaiCore.Instance.StopMusic(true);
                    }

                    // you can still stop the music in rest mode, to prevent psai from waking up again.
                    if (psaiInfo.psaiState == PsaiState.playing)
                    {
                        y += buttonHeight + spacingYsmall;

                        if (psaiInfo.upcomingPsaiState == PsaiState.silence)
                        {
                            GUI.color = new Color(0.5f + 0.5f * _flashIntensity, 0.5f + 0.5f * _flashIntensity, 0, 0.66f);
                            GUI.DrawTextureWithTexCoords(new Rect(x, y, buttonWidth, buttonHeight), _textureWhiteFull, new Rect(0, 0, 1, 1));
                        }

                        GUI.color = Color.white;
                        if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "via End-Segment", myButtonStyle))
                        {
                            PsaiCore.Instance.StopMusic(false);
                        }
                    }
                }
            }
            GUI.EndGroup();

            xPos += boxWidth + spacingX;

            GUI.BeginGroup(new Rect(xPos, yPos, boxWidth, boxHeightStandard), guiStyle);
            {
                int x = spacingX;
                int y = 0;

                bool boxIsActive = !_paused && psaiInfo.psaiState == PsaiState.playing && !PsaiCore.Instance.CutSceneIsActive() && !PsaiCore.Instance.MenuModeIsActive();
                SetGuiColorForBox(boxIsActive);

                GUI.Box(new Rect(0, 0, boxWidth, boxHeightStandard), "Return To Last Basic Mood", myBoxStyle);

                if (boxIsActive)
                {
                    y += buttonHeight;
                    if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "immediately", myButtonStyle))
                    {
                        PsaiCore.Instance.ReturnToLastBasicMood(true);
                    }

                    y += buttonHeight + spacingYsmall;

                    if (psaiInfo.returningToLastBasicMood)
                    {
                        GUI.color = new Color(0.5f + 0.5f * _flashIntensity, 0.5f + 0.5f * _flashIntensity, 0, 0.66f);
                        GUI.DrawTextureWithTexCoords(new Rect(x, y, buttonWidth, buttonHeight), _textureWhiteFull, new Rect(0, 0, 1, 1));
                    }

                    GUI.color = Color.white;
                    if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "via End-Segment", myButtonStyle))
                    {
                        PsaiCore.Instance.ReturnToLastBasicMood(false);
                    }
                }
            }
            GUI.EndGroup();

            xPos += boxWidth + spacingX;

            GUI.BeginGroup(new Rect(xPos, yPos, boxWidth, boxHeightStandard), guiStyle);
            {
                int x = spacingX;
                int y = buttonHeight;

                bool boxIsActive = psaiInfo.psaiState == PsaiState.playing;
                SetGuiColorForBox(boxIsActive);

                GUI.Box(new Rect(0, 0, boxWidth, boxHeightStandard), "", myBoxStyle);
                string buttonLabelPause;

                if (boxIsActive)
                {
                    if (psaiInfo.paused)
                    {
                        GUI.backgroundColor = Color.white;
                        buttonLabelPause = "Play";
                    }
                    else
                    {
                        GUI.backgroundColor = COLOR_DARKGREY;
                        buttonLabelPause = "Pause";
                    }

                    if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight * 2), buttonLabelPause, myButtonStyle))
                    {
                        _paused = !psaiInfo.paused;
                        PsaiCore.Instance.SetPaused(_paused);
                    }
                }
            }
            GUI.EndGroup();

            xPos += boxWidth + spacingX;



            //////////////////////////////////////////////////////////////////////////
            // Menu Mode
            //////////////////////////////////////////////////////////////////////////


            if (_configureMenuMode)
            {
                GUI.BeginGroup(new Rect(xPos, yPos, boxWidth, boxHeightSettings), guiStyle);
                {

                    bool boxIsActive = !_paused;
                    SetGuiColorForBox(boxIsActive);

                    int x = spacingX;
                    int y = 0;
                    y += buttonHeight;

                    GUI.Box(new Rect(0, 0, boxWidth, boxHeightSettings), "Menu Mode Settings", myBoxStyle);

                    if (boxIsActive)
                    {
                        if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), _themeInfos[_menuThemeId].name))
                        {
                            _menuThemeIndex++;
                            if (_menuThemeIndex == _themeIds.Length)
                            {
                                _menuThemeIndex = 0;
                            }

                            _menuThemeId = _themeIds[_menuThemeIndex];
                        }

                        y += buttonHeight;
                        GUI.Label(new Rect(x, y, buttonWidthHalf, buttonHeight), "intensity:", guiStyle);
                        GUI.Label(new Rect(x + buttonWidthHalf / 4, y + 15, buttonWidthHalf, buttonHeight), _menuThemeIntensity.ToString("F2"), guiStyle);
                        y += spacingYsmall;
                        float menuIntensitySliderValue = GUI.HorizontalSlider(new Rect(x + buttonWidthHalf, y + spacingYsmall, buttonWidthHalf, buttonHeight), _menuThemeIntensity, 0.0f, 1.0f);
                        {
                            _menuThemeIntensity = menuIntensitySliderValue;
                        }

                        y += buttonHeight;

                        if (GUI.Button(new Rect(x, y, buttonWidthHalf, buttonHeight), "OK", myButtonStyle))
                        {
                            _configureMenuMode = false;
                            _menuThemeIntensityOld = _menuThemeIntensity;
                        }
                        if (GUI.Button(new Rect(x + buttonWidthHalf, y, buttonWidthHalf, buttonHeight), "Cancel", myButtonStyle))
                        {
                            _configureMenuMode = false;
                            _menuThemeIntensity = _menuThemeIntensityOld;
                            _menuThemeId = _menuThemeIdOld;
                        }
                    }
                }
                GUI.EndGroup();

            }
            else
            {
                GUI.BeginGroup(new Rect(xPos, yPos, boxWidth, boxHeightStandard), guiStyle);
                {
                    bool boxIsActive = !_paused;
                    SetGuiColorForBox(boxIsActive);

                    int x = spacingX;
                    int y = 0;
                    y += buttonHeight;
                    GUI.Box(new Rect(0, 0, boxWidth, boxHeightStandard), "Menu Mode", myBoxStyle);

                    if (boxIsActive)
                    {
                        if (!PsaiCore.Instance.MenuModeIsActive())
                        {
                            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Enter", myButtonStyle))
                            {
                                PsaiCore.Instance.MenuModeEnter(_menuThemeId, _menuThemeIntensity);
                            }

                            if (GUI.Button(new Rect(x, y + buttonHeight + spacingYsmall, buttonWidth, buttonHeight), "Configure", myButtonStyle))
                            {
                                _configureMenuMode = true;
                                _menuThemeIdOld = _menuThemeId;
                                _menuThemeIntensityOld = _menuThemeIntensity;
                            }
                        }
                        else
                        {
                            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Leave", myButtonStyle))
                            {
                                PsaiCore.Instance.MenuModeLeave();
                            }
                        }
                    }
                }
                GUI.EndGroup();
            }

            xPos += boxWidth + spacingX;

            if (_configureCutScene)
            {
                GUI.BeginGroup(new Rect(xPos, yPos, boxWidth, boxHeightSettings), guiStyle);
                {
                    int x = spacingX;
                    int y = 0;
                    y += buttonHeight;

                    GUI.Box(new Rect(0, 0, boxWidth, boxHeightSettings), "CutScene Settings", myBoxStyle);

                    if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), _themeInfos[_cutSceneThemeId].name, myButtonStyle))
                    {
                        _cutSceneThemeIndex++;
                        if (_cutSceneThemeIndex == _themeIds.Length)
                        {
                            _cutSceneThemeIndex = 0;
                        }
                        _cutSceneThemeId = _themeIds[_cutSceneThemeIndex];
                    }

                    y += buttonHeight;

                    GUI.Label(new Rect(x, y, buttonWidthHalf, buttonHeight), "intensity:", guiStyle);
                    GUI.Label(new Rect(x + buttonWidthHalf / 4, y + 15, buttonWidthHalf, buttonHeight), _cutSceneThemeIntensity.ToString("F2"), guiStyle);
                    y += spacingYsmall;
                    float cutSceneIntensitySliderValue = GUI.HorizontalSlider(new Rect(x + buttonWidthHalf, y + spacingYsmall, buttonWidthHalf, buttonHeight), _cutSceneThemeIntensity, 0.0f, 1.0f);
                    {
                        _cutSceneThemeIntensity = cutSceneIntensitySliderValue;
                    }

                    y += buttonHeight;

                    if (GUI.Button(new Rect(x, y, buttonWidthHalf, buttonHeight), "OK", myButtonStyle))
                    {
                        _configureCutScene = false;
                        _cutSceneThemeIntensityOld = _cutSceneThemeIntensity;
                    }
                    if (GUI.Button(new Rect(x + buttonWidthHalf, y, buttonWidthHalf, buttonHeight), "Cancel", myButtonStyle))
                    {
                        _configureCutScene = false;
                        _cutSceneThemeIntensity = _cutSceneThemeIntensityOld;
                        _cutSceneThemeId = _cutSceneThemeIdOld;
                    }
                }
                GUI.EndGroup();

            }
            else
            {
                GUI.BeginGroup(new Rect(xPos, yPos, boxWidth, boxHeightStandard), guiStyle);
                {
                    int x = spacingX;
                    int y = 0;
                    y += buttonHeight;

                    bool boxIsActive = !_paused && !PsaiCore.Instance.MenuModeIsActive();
                    SetGuiColorForBox(boxIsActive);

                    GUI.Box(new Rect(0, 0, boxWidth, boxHeightStandard), "CutScene", myBoxStyle);

                    if (boxIsActive)
                    {
                        if (!PsaiCore.Instance.CutSceneIsActive())
                        {
                            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Enter", myButtonStyle))
                            {
                                PsaiCore.Instance.CutSceneEnter(_cutSceneThemeId, _cutSceneThemeIntensity);
                            }

                            if (GUI.Button(new Rect(x, y + buttonHeight + spacingYsmall, buttonWidth, buttonHeight), "Configure", myButtonStyle))
                            {
                                _configureCutScene = true;
                                _cutSceneThemeIdOld = _cutSceneThemeId;
                                _cutSceneThemeIntensityOld = _cutSceneThemeIntensity;
                            }
                        }
                        else
                        {
                            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Leave immediately", myButtonStyle))
                            {
                                PsaiCore.Instance.CutSceneLeave(true, false);
                            }

                            y += buttonHeight + spacingYsmall;

                            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Leave smoothly", myButtonStyle))
                            {
                                PsaiCore.Instance.CutSceneLeave(false, false);
                            }
                        }
                    }
                }
                GUI.EndGroup();
            }


            GUI.EndScrollView();

            yPos = yPosOld;
        }


        GUI.color = Color.white;
        yPos += boxHeightSettings;
        yPos += spacingYsmall * 2;

        int playbackStatsGroupOffsetX = spacingX;
        xPos = playbackStatsGroupOffsetX;

        int playbackStatsGroupWidth = (int)(Screen.width / 2.0f) - playbackStatsGroupOffsetX - spacingX;
        int playbackStatsGroupHeight = (int)spacingY * 7;

        if (currentThemeChangedInThisFrame)
        {
            _selectedThemeId = PsaiCore.Instance.GetCurrentThemeId();
        }


        //////////////////////////////////////////////////////////////////////////
        // Bottom Section
        //////////////////////////////////////////////////////////////////////////


        //////////////////////////////////////////////////////////////////////////
        // Toggle View Button
        //////////////////////////////////////////////////////////////////////////

        //int playStateInfoBoxHeight = (FONTSIZE + spacingY) * 6;

        int segmentInfoSectionOffsetX = playbackStatsGroupOffsetX + playbackStatsGroupWidth + spacingX;

        if (_showListSections)
        {
            xPos = spacingX;

            string buttonLabel = (_showListView == true) ? "Listview" : "Playstate";
            if (GUI.Button(new Rect(xPos, yPos, buttonWidthHalf, buttonHeight), buttonLabel, myButtonStyle))
            {
                _showListView = !_showListView;
                _switchedToListViewInCurrentFrame = _showListView;            
            }          

            yPos += buttonHeight;            

            //////////////////////////////////////////////////////////////////////
            /// Theme / Segment Info Scrollview
            //////////////////////////////////////////////////////////////////////
            xPos = playbackStatsGroupOffsetX;

            if (_showListView)
            {
                int themeListViewWidth = playbackStatsGroupWidth;
                int segmentListViewWidth = Screen.width - segmentInfoSectionOffsetX - spacingX;

                int listViewHeight = (int)((Screen.height - yPos) * 0.90f);
                int lineHeight = 22;
                int themeListContentHeight = Mathf.Max(_themeInfos.Keys.Count * lineHeight, listViewHeight);



                //yPos = Screen.height - themeListContentHeight;

                GUI.Label(new Rect(xPos + themeListViewWidth / 2, yPos - lineHeight, themeListViewWidth, lineHeight), "Themes", guiStyle);
                GUI.Box(new Rect(xPos, yPos, themeListViewWidth, listViewHeight), "", myBoxStyle);


                //
                /// Themelist Column Headlines
                //

                for (int columnIndex = 0; columnIndex < _columnWidthsRatiosThemeList.Length; columnIndex++)
                {
                    int columnWidth = themeListViewWidth / _columnWidthsRatiosThemeList[columnIndex];
                    if (GUI.Button(new Rect(xPos, yPos, columnWidth, buttonHeight), _columnNamesThemeList[columnIndex], "Label"))
                    {
                        switch (columnIndex)
                        {
                            case 0:
                                _themeIdsList.Sort(CompareThemesByName);
                                break;

                            case 1:
                                _themeIdsList.Sort();
                                break;

                            case 2:
                                _themeIdsList.Sort(CompareThemesByType);
                                break;
                        }

                        _sortThemesAscending = !_sortThemesAscending;

                        if (_sortThemesAscending)
                        {
                            _themeIdsList.Reverse();
                        }

                    }
                    xPos += columnWidth;
                }
                xPos = playbackStatsGroupOffsetX;

                GUI.BeginGroup(new Rect(xPos, yPos, themeListViewWidth, listViewHeight), guiStyle);
                {
                    _scrollPositionThemeList = GUI.BeginScrollView(new Rect(0, lineHeight, themeListViewWidth, listViewHeight - lineHeight), _scrollPositionThemeList, new Rect(0, 0, themeListViewWidth - spacingX * 2, themeListContentHeight), false, true);
                    int y = 0;
                    for (int themeIndex = 0; themeIndex < _themeIds.Length; themeIndex++)
                    {
                        ThemeInfo themeInfo = _themeInfos[_themeIdsList[themeIndex]];

                        if (_selectedThemeId == themeInfo.id)
                        {
                            GUI.color = Color.white;
                            GUI.DrawTextureWithTexCoords(new Rect(0, y, themeListViewWidth, lineHeight), _textureWhiteAlpha, new Rect(0, 0, 1, 1));
                        }

                        if (themeInfo.id == currentThemeId)
                        {
                            GUI.color = COLOR_CURRENT_THEME;
                        }
                        else if (themeInfo.id == upcomingThemeId)
                        {
                            GUI.color = COLOR_UPCOMING_THEME;
                        }
                        else if (_selectedThemeId == themeInfo.id)
                        {
                            GUI.color = Color.white;
                        }
                        else
                        {
                            GUI.color = COLOR_LIGHTGREY;
                        }


                        int xOffsetColumn = 0;
                        for (int columnIndex = 0; columnIndex < _columnWidthsRatiosThemeList.Length; columnIndex++)
                        {
                            int columnWidth = themeListViewWidth / _columnWidthsRatiosThemeList[columnIndex];
                            string columnString = "";
                            switch (columnIndex)
                            {
                                case 0:
                                    columnString = themeInfo.name;
                                    break;

                                case 1:
                                    columnString = themeInfo.id.ToString();
                                    break;

                                case 2:
                                    columnString = psai.net.Theme.ThemeTypeToString(themeInfo.type);
                                    break;
                            }
                            if (GUI.Button(new Rect(xOffsetColumn, y, themeListViewWidth, lineHeight), columnString, "Label"))
                            {
                                _selectedThemeId = themeInfo.id;
                            }

                            xOffsetColumn += columnWidth;
                        }
                        y += lineHeight;
                    }
                    GUI.EndScrollView();
                }
                GUI.EndGroup();

                xPos = segmentInfoSectionOffsetX;


                //////////////////////////////////////////////////////////////////////////
                // SegmentList
                //////////////////////////////////////////////////////////////////////////

                GUI.color = Color.white;
                GUI.Label(new Rect(xPos + segmentListViewWidth / 2, yPos - lineHeight, segmentListViewWidth, lineHeight), "Segments", guiStyle);
                GUI.Box(new Rect(xPos, yPos, segmentListViewWidth, listViewHeight), "", myBoxStyle);


                if (_themeInfos.ContainsKey(_selectedThemeId))
                {
                    int[] segmentIds = _themeInfos[_selectedThemeId].segmentIds;
                    bool selectedThemeChangedInThisFrame = (_selectedThemeId != _selectedThemeIdInLastFrame);
                    if (selectedThemeChangedInThisFrame || _switchedToListViewInCurrentFrame || _highlightTriggered)
                    {
                        _segmentIdsListOfSelectedTheme.Clear();
                        _segmentIdsListOfSelectedTheme.AddRange(segmentIds);

                        RebuildSegmentInfoCache();

                        _highlightTriggered = false;
                    }

                    int segmentListContentHeight = Mathf.Max(_themeInfos[_selectedThemeId].segmentIds.Length * lineHeight, listViewHeight);

                    //
                    /// Column Headlines
                    //
                    for (int columnIndex = 0; columnIndex < _columnWidthsRatiosSegments.Length; columnIndex++)
                    {
                        int columnWidth = segmentListViewWidth / _columnWidthsRatiosSegments[columnIndex];
                        if (GUI.Button(new Rect(xPos, yPos, columnWidth, buttonHeight), _columnNamesSegments[columnIndex], "Label"))
                        {

                            switch (columnIndex)
                            {
                                case 0:
                                    _segmentIdsListOfSelectedTheme.Sort(CompareSegmentsByName);
                                    break;

                                case 1:
                                    _segmentIdsListOfSelectedTheme.Sort(CompareSegmentsBySuitablilies);
                                    break;

                                case 2:
                                    _segmentIdsListOfSelectedTheme.Sort(CompareSegmentsByIntensity);
                                    break;

                                case 3:
                                    _segmentIdsListOfSelectedTheme.Sort(CompareSegmentsByPlaycount);
                                    break;
                            }

                            _sortSegmentsAscending = !_sortSegmentsAscending;

                            if (_sortSegmentsAscending)
                            {
                                _segmentIdsListOfSelectedTheme.Reverse();
                            }

                        }
                        xPos += columnWidth;
                    }

                    GUI.BeginGroup(new Rect(segmentInfoSectionOffsetX, yPos, segmentListViewWidth, listViewHeight), guiStyle);
                    {
                        _scrollPositionSegmenttList = GUI.BeginScrollView(new Rect(0, lineHeight, segmentListViewWidth, listViewHeight - lineHeight), _scrollPositionSegmenttList, new Rect(0, 0, segmentListViewWidth - spacingX * 2, segmentListContentHeight), false, true);

                        int y = 0;
                        for (int segmentIndex = 0; segmentIndex < _segmentIdsListOfSelectedTheme.Count; segmentIndex++)
                        {
                            int segmentId = _segmentIdsListOfSelectedTheme[segmentIndex];
                            if (segmentId == currentSegmentId)
                            {
                                GUI.color = COLOR_CURRENT_THEME;

                                // make sure the playing snippet is visible
                                if (currentSegmentChangedInThisFrame && _autoScrollToCurrentSegment)
                                {
                                    _scrollPositionSegmenttList.y = segmentIndex * lineHeight;
                                }
                            }
                            else if (segmentId == psaiInfo.targetSegmentId)
                            {
                                GUI.color = COLOR_UPCOMING_THEME;
                            }
                            else
                            {
                                GUI.color = COLOR_LIGHTGREY;
                            }

                            if (_playbackCountdowns.ContainsKey(segmentId) && _playbackCountdowns[segmentId] > 0)
                            {
                                GUI.color = COLOR_CURRENT_THEME;
                            }

                            SegmentInfo segmentInfo = _segmentInfos[segmentId];

                            int xOffsetColumn = 0;
                            for (int columnIndex = 0; columnIndex < _columnWidthsRatiosSegments.Length; columnIndex++)
                            {
                                string columnString = "";
                                switch (columnIndex)
                                {
                                    case 0: columnString = segmentInfo.name;
                                        break;
                                    case 1:
                                        columnString = Segment.GetStringFromSegmentSuitabilities(segmentInfo.segmentSuitabilitiesBitfield);
                                        break;
                                    case 2:
                                        columnString = segmentInfo.intensity.ToString("F2");
                                        break;
                                    case 3:
                                        columnString = segmentInfo.playcount.ToString();
                                        break;
                                }

                                int columnNameWidth = segmentListViewWidth / _columnWidthsRatiosSegments[columnIndex];
                                //GUI.Label(new Rect(xOffsetColumn, y, columnNameWidth, lineHeight), columnString);

                                if (GUI.Button(new Rect(xOffsetColumn, y, columnNameWidth, lineHeight), columnString, "Label"))
                                {
                                    PsaiCore.Instance.PlaySegment(segmentId);
                                    StorePlaybackCountdownForSnippet(segmentId);
                                }

                                xOffsetColumn += columnNameWidth;
                            }

                            y += lineHeight;
                        }
                        GUI.EndScrollView();

                    }
                    GUI.EndGroup();
                }
            }
            else
            {

                //////////////////////////////////////////////////////////////////////////
                // Theme Info Box
                //////////////////////////////////////////////////////////////////////////

                int middleYpos = yPos;

                yPos = Mathf.Max(middleYpos, Screen.height - playbackStatsGroupHeight - this._textureNeedMusic.height - buttonHeight- spacingY);

                if (psaiInfo.psaiState == PsaiState.playing)
                {
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = COLOR_DARKGREY;
                }
                
                GUI.BeginGroup(new Rect(xPos, yPos, playbackStatsGroupWidth, playbackStatsGroupHeight), guiStyle);
                {
                    int x = spacingX;
                    int y = 0;
                    int middleX = (int)(playbackStatsGroupWidth / 2.0f);   

                    GUI.Box(new Rect(0, 0, playbackStatsGroupWidth, playbackStatsGroupHeight), "current Theme playing", myBoxStyle);

                    x += spacingX;
                    y += spacingY;

                    string strThemeName = "";
                    string strThemeId = "";
                    string strThemeType = "";

                    if (currentThemeId > 0)
                    {
                        ThemeInfo themeInfo = _themeInfos[currentThemeId];
                        strThemeName = themeInfo.name;
                        strThemeId = themeInfo.id.ToString();
                        strThemeType = psai.net.Theme.ThemeTypeToString(themeInfo.type);
                    }

                    GUI.Label(new Rect(x, y, playbackStatsGroupWidth, spacingY), "Name: ", guiStyle);
                    GUI.Label(new Rect(middleX, y, playbackStatsGroupWidth, spacingY), strThemeName, guiStyle);
                    y += spacingY;

                    GUI.Label(new Rect(x, y, playbackStatsGroupWidth, spacingY), "id: ", guiStyle);
                    GUI.Label(new Rect(middleX, y, playbackStatsGroupWidth, spacingY), strThemeId, guiStyle);
                    y += spacingY;

                    GUI.Label(new Rect(x, y, playbackStatsGroupWidth, spacingY), "ThemeType: ", guiStyle);
                    GUI.Label(new Rect(middleX, y, playbackStatsGroupWidth, spacingY), strThemeType, guiStyle);

                    y += spacingY;
                    y += spacingY;
                    y += spacingY;

                    GUI.Label(new Rect(x, y, playbackStatsGroupWidth, spacingY), "psai state: " + psaiInfo.psaiState.ToString(), guiStyle);
                    if (psaiInfo.remainingMillisecondsInRestMode > 0)
                    {
                        string wakeUpString = "Waking up in: " + psaiInfo.remainingMillisecondsInRestMode.ToString();
                        GUI.Label(new Rect(middleX, y, playbackStatsGroupWidth, spacingY), wakeUpString, guiStyle);
                    }
                }
                GUI.EndGroup();

                //////////////////////////////////////////////////////////////////////////
                // Segment Info Box
                //////////////////////////////////////////////////////////////////////////
                xPos = segmentInfoSectionOffsetX;

                int snippetGroupWidth = playbackStatsGroupWidth;
                int snippetGroupHeight = playbackStatsGroupHeight;

                GUI.BeginGroup(new Rect(xPos, yPos, snippetGroupWidth, snippetGroupHeight), guiStyle);
                {
                    int xSegment = spacingX;
                    int ySegment = 0;
                    int middleXsegmentGroup = (int)(snippetGroupWidth / 2);

                    GUI.Box(new Rect(0, 0, snippetGroupWidth, snippetGroupHeight), "current Segment playing", myBoxStyle);

                    xSegment += spacingX;
                    ySegment += spacingY;

                    string strSegmentName = "";
                    string strSegmentId = "";
                    string strIntensity = "";
                    string strSegmentType = "";
                    string strSegmentPlaycount = "";

                    if (currentSegmentId > 0)
                    {
                        SegmentInfo segmentInfo = _segmentInfos[currentSegmentId];
                        strSegmentName = segmentInfo.name;
                        strSegmentId = segmentInfo.id.ToString();
                        strIntensity = segmentInfo.intensity.ToString("F2");
                        strSegmentType = Segment.GetStringFromSegmentSuitabilities(segmentInfo.segmentSuitabilitiesBitfield);
                        strSegmentPlaycount = segmentInfo.playcount.ToString();
                    }

                    GUI.Label(new Rect(xSegment, ySegment, playbackStatsGroupWidth, spacingY), "Name: ", guiStyle);
                    GUI.Label(new Rect(middleXsegmentGroup, ySegment, playbackStatsGroupWidth, spacingY), strSegmentName);
                    ySegment += spacingY;

                    GUI.Label(new Rect(xSegment, ySegment, playbackStatsGroupWidth, spacingY), "id: ", guiStyle);
                    GUI.Label(new Rect(middleXsegmentGroup, ySegment, playbackStatsGroupWidth, spacingY), strSegmentId);
                    ySegment += spacingY;

                    GUI.Label(new Rect(xSegment, ySegment, playbackStatsGroupWidth, spacingY), "Intensity: ", guiStyle);
                    GUI.Label(new Rect(middleXsegmentGroup, ySegment, playbackStatsGroupWidth, spacingY), strIntensity);
                    ySegment += spacingY;

                    GUI.Label(new Rect(xSegment, ySegment, playbackStatsGroupWidth, spacingY), "Suitabilities: ", guiStyle);
                    GUI.Label(new Rect(middleXsegmentGroup, ySegment, playbackStatsGroupWidth, spacingY), strSegmentType);
                    ySegment += spacingY;

                    GUI.Label(new Rect(xSegment, ySegment, playbackStatsGroupWidth, spacingY), "times played: ");
                    GUI.Label(new Rect(middleXsegmentGroup, ySegment, playbackStatsGroupWidth, spacingY), strSegmentPlaycount);
                    ySegment += spacingY;

                    string strRemainingMillisOfCurrentSnippet = PsaiCore.Instance.GetRemainingMillisecondsOfCurrentSegmentPlayback().ToString();
                    GUI.Label(new Rect(xSegment, ySegment, snippetGroupWidth, spacingY), "remaining ms: ", guiStyle);
                    GUI.Label(new Rect(middleXsegmentGroup, ySegment, snippetGroupWidth, spacingY), strRemainingMillisOfCurrentSnippet, guiStyle);
                    ySegment += spacingY;

                    string strCountdownUntilNextSnippet = PsaiCore.Instance.GetRemainingMillisecondsUntilNextSegmentStart().ToString();
                    GUI.Label(new Rect(xSegment, ySegment, snippetGroupWidth, spacingY), "ms until next Segment: ", guiStyle);
                    GUI.Label(new Rect(middleXsegmentGroup, ySegment, snippetGroupWidth, spacingY), strCountdownUntilNextSnippet, guiStyle);
                }
                GUI.EndGroup();


                // Sonic Liberty Logo
                if (_textureNeedMusic != null)
                {
                    GUI.color = Color.white;
                    yPos += playbackStatsGroupHeight;
                    yPos += spacingY;
                    //yPos += spacingYsmall;
                    GUI.DrawTextureWithTexCoords(new Rect(Screen.width - _textureNeedMusic.width - 22, yPos, _textureNeedMusic.width, _textureNeedMusic.height), _textureNeedMusic, new Rect(0, 0, 1, 1));
                }
            }

            xPos = spacingX;

            yPos = Screen.height - (guiStyle.fontSize + 5);
            if (_lastErrorString != null && _lastErrorString.Length > 0)
            {
                GUI.color = COLOR_LIGHTRED;
                GUI.Label(new Rect(xPos, yPos, playbackStatsGroupWidth, guiStyle.fontSize), "last error: " + _lastErrorString, guiStyle);
            }

            _playingThemeIdInLastFrame = currentThemeId;
            _playingSegmentIdInLastFrame = currentSegmentId;
            _selectedThemeIdInLastFrame = _selectedThemeId;
            _switchedToListViewInCurrentFrame = false;


            // decrement countdowns
            if (_playbackCountdowns.Count > 0)
            {
                List<int> snippetIdsToRemove = new List<int>();
                int[] countdownKeysList = new int[_playbackCountdowns.Count];
                _playbackCountdowns.Keys.CopyTo(countdownKeysList, 0);

                foreach (int snippetId in countdownKeysList)
                {
                    int countDownMs = _playbackCountdowns[snippetId];
                    countDownMs -= (int)(Time.deltaTime * 1000.0f);

                    if (countDownMs > 0)
                        _playbackCountdowns[snippetId] = countDownMs;
                    else
                        snippetIdsToRemove.Add(snippetId);
                }

                foreach (int snippetId in snippetIdsToRemove)
                {
                    _playbackCountdowns.Remove(snippetId);
                }
            }
        }


    }

    private void StorePlaybackCountdownForSnippet(int snippetId)
    {
        if (_segmentInfos.ContainsKey(snippetId))
        {
            SegmentInfo snippetInfo = _segmentInfos[snippetId];
            _playbackCountdowns[snippetId] = snippetInfo.fullLengthInMilliseconds;
        }        
    }

    private void RebuildSegmentInfoCache()
    {
        _segmentInfos.Clear();
        foreach (int themeId in _themeIds)
        {
            ThemeInfo themeInfo = PsaiCore.Instance.GetThemeInfo(themeId);
            int[] segmentIdsz = themeInfo.segmentIds;
            foreach (int segmentId in segmentIdsz)
            {
                SegmentInfo segmentInfo = PsaiCore.Instance.GetSegmentInfo(segmentId);
                _segmentInfos[segmentId] = segmentInfo;
            }
        }
    }
}

