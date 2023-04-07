// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if (UNITY_EDITOR)
using System.IO;
namespace Niantic.LightshipHub
{
  public class LightshipWelcomeWindow : EditorWindow
  {
    private static VisualElement _root;
    private ScrollView _templates;
    private ScrollView _projects;
    private ScrollView _uikit;
    private ScrollView _aboutUs;
    private ScrollView _help;


    public static void ShowWindow()
    {
      var window = GetWindow<LightshipWelcomeWindow>();
      window.titleContent = new GUIContent("Welcome to Lightship Templates");
      window.minSize = new Vector2(970, 680);
      window.maxSize = new Vector2(970, 680);
      window.Show();
    }

    private void OnEnable()
    {
      VisualTreeAsset design = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/LightShipHUB/Editor/Scripts/LightshipWelcomeWindow.uxml");
      TemplateContainer structure = design.CloneTree();
      rootVisualElement.Add(structure);
      StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/LightShipHUB/Editor/Scripts/LightshipWelcomeWindowStyles.uss");
      rootVisualElement.styleSheets.Add(style);
      _root = rootVisualElement;
      AddLogo();
      CreateMenu();
      CreateTemplates();
      CreateSampleProjects();
      CreateUITools();
      CreateHelp();
    }

    private void AddLogo()
    {
    }

    private void ChangeAddedStatus()
    {

    }

    public void ShowTemplates()
    {
      // _templates.RemoveFromClassList("hide");
      // _projects.AddToClassList("hide");
      // _aboutUs.AddToClassList("hide");
      // _help.AddToClassList("hide");
    }

    public void ShowProjects()
    {
      // _templates.AddToClassList("hide");
      // _projects.RemoveFromClassList("hide");
      // _aboutUs.AddToClassList("hide");
      // _help.AddToClassList("hide");
    }

    public void ShowAboutUs()
    {
      // _templates.AddToClassList("hide");
      // _projects.AddToClassList("hide");
      // _aboutUs.RemoveFromClassList("hide");
      // _help.AddToClassList("hide");
    }

    public void ShowHelp()
    {
      // _templates.AddToClassList("hide");
      // _projects.AddToClassList("hide");
      // _aboutUs.AddToClassList("hide");
      // _help.RemoveFromClassList("hide");
    }

    public void ShowUIKit(){

      // _templates.AddToClassList("hide");
      // _projects.AddToClassList("hide");
      // _aboutUs.AddToClassList("hide");
      // _help.RemoveFromClassList("hide");
    }

    private string GetVersions()
    {
      //Read the text from directly from the test.txt file
      string versions = "Versions: \n";
      StreamReader HUBVersion_reader = new StreamReader("Assets/LightshipHUB/VERSION"); 
      versions += "HUB "+HUBVersion_reader.ReadToEnd()+"\n";
      HUBVersion_reader.Close();

      StreamReader ARDKVersion_reader = new StreamReader("Assets/ARDK/VERSION"); 
      versions += "ARDK "+ARDKVersion_reader.ReadToEnd();
      ARDKVersion_reader.Close();
      return versions;
    }

    public static void openHelp(){

      var window = GetWindow<LightshipWelcomeWindow>();
      window.Show();
      window.UpdateMenu("menuHelp");
    }

    private void UpdateMenu( string menuItem ){
      string[] menuItems = new string[5]{"menuTemplates", "menuProjects", "menuUIKit", "menuAbout", "menuHelp"};
      ScrollView[] sections = new ScrollView[5]{_templates, _projects, _uikit, _aboutUs, _help};
      for(int i = 0; i < menuItems.Length; i++){
        string item = menuItems[i];
        ScrollView section = sections[i];
        if(item==menuItem){
          rootVisualElement.Query<Label>(item).First().AddToClassList("active");
          section.RemoveFromClassList("hide");
        }else{
          rootVisualElement.Query<Label>(item).First().RemoveFromClassList("active");
          section.AddToClassList("hide");
        }
      }
    }
    private void CreateMenu()
    {
      rootVisualElement.Query<Label>("version").First().text = GetVersions();

      _templates = _root.Query<ScrollView>("tutorials").First() as ScrollView;
      _projects = _root.Query<ScrollView>("projects").First() as ScrollView;
      _uikit = _root.Query<ScrollView>("uikit").First() as ScrollView;
      _aboutUs = _root.Query<ScrollView>("aboutus").First() as ScrollView;
      _help = _root.Query<ScrollView>("help").First() as ScrollView;
      
      Label templatesBtn = rootVisualElement.Query<Label>("menuTemplates").First();
      templatesBtn.RegisterCallback<MouseDownEvent>(evt => {
        UpdateMenu("menuTemplates");
        // ShowTemplates();
      });

      Label projectsBtn = rootVisualElement.Query<Label>("menuProjects").First();
      projectsBtn.RegisterCallback<MouseDownEvent>(evt => {
        UpdateMenu("menuProjects");
        // ShowProjects();
      });

      Label uikitBtn = rootVisualElement.Query<Label>("menuUIKit").First();
      uikitBtn.RegisterCallback<MouseDownEvent>(evt => {
        UpdateMenu("menuUIKit");
        // ShowProjects();
      });

      Label aboutBtn = rootVisualElement.Query<Label>("menuAbout").First();
      aboutBtn.RegisterCallback<MouseDownEvent>(evt => {
        UpdateMenu("menuAbout");
        // ShowAboutUs();
      });

      Label helpBtn = rootVisualElement.Query<Label>("menuHelp").First();
      helpBtn.RegisterCallback<MouseDownEvent>(evt => {
        UpdateMenu("menuHelp");
        // ShowHelp();
      });
    }

    private void CreateTemplates()
    {
      // rootVisualElement.Query<Box>("tuto1_1").First().RegisterCallback<MouseDownEvent>(evt => {
      //   TemplateFactory.CreateTemplate_AnchorPlacement();
      //   this.Close();
      // });
      // rootVisualElement.Query<Box>("tuto1_2").First().RegisterCallback<MouseDownEvent>(evt => {
      //   TemplateFactory.CreateTemplate_AnchorPlacementWithoutPlanes();
      //   this.Close();
      // });
      // rootVisualElement.Query<Box>("tuto1_2").First().RegisterCallback<MouseDownEvent>( evt=>{
      //     TemplateFactory.CreateTemplate_AnchorInteraction();
      //     this.Close();
      // });
      rootVisualElement.Query<Box>("tuto1_3").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_PlaneTracker(false);
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto1_4").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_ImageDetection();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_1").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_DepthTextureOcclusion();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_2").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_MeshOcclusion();
        this.Close();
      });
      // rootVisualElement.Query<Box>("tuto2_3").First().RegisterCallback<MouseDownEvent>(evt => {
      //   TemplateFactory.CreateTemplate_RealtimeMeshing();
      //   this.Close();
      // });
      // rootVisualElement.Query<Box>("tuto2_4").First().RegisterCallback<MouseDownEvent>(evt => {
      //   TemplateFactory.CreateTemplate_MeshCollider();
      //   this.Close();
      // });
      rootVisualElement.Query<Box>("tuto2_5").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_MeshingShaders();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_6").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_MeshGarden();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_7").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_AdvancedPhysics();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto2_8").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_CharacterController();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto3_1").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_SemanticSegmentation();
        this.Close();
      });
      // rootVisualElement.Query<Box>("tuto3_2").First().RegisterCallback<MouseDownEvent>( evt=>{
      //     TemplateFactory.CreateTemplate_ObjectMasking();
      //     this.Close();
      // });
      // rootVisualElement.Query<Box>("tuto3_3").First().RegisterCallback<MouseDownEvent>(evt => {
      //   TemplateFactory.CreateTemplate_OptimizedObjectMasking();
      //   this.Close();
      // });
      rootVisualElement.Query<Box>("tuto4_1").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_SharedObjectInteraction();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto5_1").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_VPSCoverage();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto5_2").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_VPSCoverageList();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto5_3").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_WayspotAnchors();
        this.Close();
      });
      rootVisualElement.Query<Box>("tuto5_4").First().RegisterCallback<MouseDownEvent>(evt => {
        TemplateFactory.CreateTemplate_LeaveMessages();
        this.Close();
      });
    }

    public void CreateSampleProjects()
    {
      rootVisualElement.Query<Box>("project_ARHockey").First().RegisterCallback<MouseDownEvent>(evt => {
        SamplesFactory.OpenSampleProject_ARHockey();
        this.Close();
      });
    }

    public void CreateUITools()
    {
      rootVisualElement.Query<Box>("uibutton").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Button.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uibuttonsec").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/ButtonWithBorder.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uiicons").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/ButtonIcon.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uicheckbox").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Checkbox.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uiswitch").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Switch.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uislider").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Slider.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uiinput").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/InputField.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uidropdown").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Dropdown.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uilabel").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Label.prefab");
        this.ChangeAddedStatus();
      });

      rootVisualElement.Query<Box>("uinetworkstatus").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/NetworkStatusIndicator.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uitopstatusbar").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/TopStatusBar.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uilog").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/ScrollingLog.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uimovedevice").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/MoveDeviceIndicator.prefab");
        this.ChangeAddedStatus();
      });

      rootVisualElement.Query<Box>("uipanel").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Screens/Panel.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uiloader").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Screens/Loading.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uisyncstate").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Screens/SyncStateHelperScreen.prefab");
        this.ChangeAddedStatus();
      });
      rootVisualElement.Query<Box>("uiyourlogoscreen").First().RegisterCallback<MouseDownEvent>(evt => {
        LightshipUIFactory.CreateUIComponent("Assets/LightshipHUB/Runtime/Prefabs/Tools/UI/Screens/YourLogoScreen.prefab");
        this.ChangeAddedStatus();
      });
    }

    public void buildHelp(string[] ids, string[] links){
      for(int i = 0; i < ids.Length; i++){
        string id = ids[i];
        string link = links[i];
        rootVisualElement.Query<Box>(id).First().RegisterCallback<MouseDownEvent>(evt => {
          Application.OpenURL(link);
        });
      }
    }

    public void CreateHelp()
    {

      string[] help_ids = new string[4]{
        // "learn_more",
        "help_iosbuild",
        "help_androidbuild",
        "help_gettingstarted",
        "help_documentation"
      };
      string[] help_links = new string[4]{
        // "https://lightship.dev/",
        "https://lightship.dev/docs/ardk/ardk_fundamentals/building_ios.html",
        "https://lightship.dev/docs/ardk/ardk_fundamentals/building_android.html",
        "https://lightship.dev/docs/ardk/ardk_fundamentals/getting_started.html",
        "https://lightship.dev/docs/ardk/index.html"
      };


      string[] atlas_ids = new string[2]{
        "help_installingardk",
        "help_glossary"
      };
      string[] atlas_links = new string[2]{
        "https://lightship.dev/learn/beginners-atlas/install-ardk/",
        "https://lightship.dev/learn/beginners-atlas/glossary/"
      };
      string[] guides_ids = new string[9]{
        "help_guideshome",
        "help_thebasics",
        "help_usingtemplates",
        "help_usingvirtualstudio",
        "help_meshing",
        "help_sharedarsession",
        "help_sharedarhostpeers",
        "help_scanningforvps",
        "help_buildingforvps"
      };
      string[] guides_links = new string[9]{
        "https://lightship.dev/guides/",
        "https://lightship.dev/guides/lightship-basics/",
        "https://lightship.dev/guides/getting-started-lightship-templates/",
        "https://lightship.dev/guides/using-virtual-studio/",
        "https://lightship.dev/guides/meshing/",
        "https://lightship.dev/guides/shared-ar-session-plane-tracking/",
        "https://lightship.dev/guides/shared-ar-host-peer-communication/",
        "https://lightship.dev/guides/scanning-for-vps/",
        "https://lightship.dev/guides/building-with-vps/"
      };


      string[] templates_ids = new string[20]{
        "help_templateshome",
        "help_objectplacement",
        "help_planetracker",
        "help_objectplacementwithoutplanes",
        "help_imagedetection",
        "help_textureocclusion",
        "help_meshocclusion",
        "help_realtimemeshing",
        "help_meshcollider",
        "help_semanticsegmentation",
        "help_semanticmasking",
        "help_meshingshaders",
        "help_meshgarden",
        "help_advancedphysics",
        "help_charactercontroller",
        "help_sharedar",
        "help_vpscoverageimage",
        "help_vpscoveragelist",
        "help_wayspotanchors",
        "help_leavemessages"
      };
      string[] templates_links = new string[20]{
        "https://lightship.dev/learn/templates",
        "https://lightship.dev/templates/object-placement/",
        "https://lightship.dev/templates/plane-tracker/",
        "https://lightship.dev/templates/object-placement-without-planes/",
        "https://lightship.dev/templates/image-detection/",
        "https://lightship.dev/templates/texture-occulsion/",
        "https://lightship.dev/templates/mesh-occlusion/",
        "https://lightship.dev/templates/real-time-meshing/",
        "https://lightship.dev/templates/mesh-collider/",
        "https://lightship.dev/templates/semantic-segmentation/",
        "https://lightship.dev/templates/semantic-masking/",
        "https://lightship.dev/templates/meshing-shaders/",
        "https://lightship.dev/templates/mesh-garden/",
        "https://lightship.dev/templates/advanced-physics/",
        "https://lightship.dev/templates/character-controller/",
        "https://lightship.dev/templates/shared-object-interaction/",
        "https://lightship.dev/templates/vps-coverage-image/",
        "https://lightship.dev/templates/vps-coverage-list/",
        "https://lightship.dev/templates/wayspot-anchors/",
        "https://lightship.dev/templates/leave-messages/"
      };

      buildHelp(help_ids,help_links);
      buildHelp(atlas_ids,atlas_links);
      buildHelp(guides_ids,guides_links);
      buildHelp(templates_ids,templates_links);

      rootVisualElement.Query<Label>("learn_more").First().RegisterCallback<MouseDownEvent>(evt => {
        Application.OpenURL("https://lightship.dev/");
      });

      // rootVisualElement.Query<Box>("help_iosbuild").First().RegisterCallback<MouseDownEvent>(evt => {
      //   Application.OpenURL("https://lightship.dev/docs/ardk/ardk_fundamentals/building_ios.html");
      // });

      // rootVisualElement.Query<Box>("help_androidbuild").First().RegisterCallback<MouseDownEvent>(evt => {
      //   Application.OpenURL("https://lightship.dev/docs/ardk/ardk_fundamentals/building_android.html");
      // });

      // rootVisualElement.Query<Box>("help_gettingstarted").First().RegisterCallback<MouseDownEvent>(evt => {
      //   Application.OpenURL("https://lightship.dev/docs/ardk/ardk_fundamentals/getting_started.html");
      // });

      // rootVisualElement.Query<Box>("help_documentation").First().RegisterCallback<MouseDownEvent>(evt => {
      //   Application.OpenURL("https://lightship.dev/docs/ardk/index.html");
      // });
    }

    public void SendMouseUpEventTo(IEventHandler handler)
    {
      using (var mouseUp = MouseUpEvent.GetPooled(new Event()))
      {
        mouseUp.target = handler;
        handler.SendEvent(mouseUp);
      }
    }
  }
}
#endif