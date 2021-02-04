using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Animations;
using HelixToolkit.Wpf.SharpDX.Assimp;
using HelixToolkit.Wpf.SharpDX.Controls;
using HelixToolkit.Wpf.SharpDX.Model;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;



namespace MyHelixPractise
{

    using Media3D = System.Windows.Media.Media3D;


    public partial class MainViewModel : BaseViewModel
    {

        private MainWindow mainWindow = null;

        private CancellationTokenSource cts = new CancellationTokenSource();

        private bool reset = true;

        private HelixToolkitScene scene;

        private List<BoneSkinMeshNode> boneSkinNodes = new List<BoneSkinMeshNode>();

        private List<BoneSkinMeshNode> skeletonNodes = new List<BoneSkinMeshNode>();

        public ObservableCollection<Animation> Animations { get; } = new ObservableCollection<Animation>();

        public SceneNodeGroupModel3D GroupModel { get; } = new SceneNodeGroupModel3D();


        private CompositionTargetEx compositeHelper = new CompositionTargetEx();

        
        private bool showWireframe = false;
        public bool ShowWireframe
        {
            set
            {
                if (SetValue(ref showWireframe, value))
                {
                    foreach (var m in boneSkinNodes)
                    {
                        m.RenderWireframe = value;
                    }
                }
            }
            get
            {
                return showWireframe;
            }
        }

        private bool showSkeleton = false;
        public bool ShowSkeleton
        {
            set
            {
                if (SetValue(ref showSkeleton, value))
                {
                    foreach (var m in skeletonNodes)
                    {
                        m.Visible = value;
                    }
                }
            }
            get
            {
                return showSkeleton;
            }
        }



        private bool enableAnimation = true;
        public bool EnableAnimation
        {
            set
            {
                enableAnimation = value;

                OnPropertyChanged();

                if (enableAnimation)
                {
                    StartAnimation();
                }
                else
                {
                    StopAnimation();
                }
            }
            get
            {
                return enableAnimation;
            }
        }

        public void StartAnimation()
        {
            compositeHelper.Rendering += CompositeHelper_Rendering;
        }

        public void StopAnimation()
        {
            compositeHelper.Rendering -= CompositeHelper_Rendering;
        }



        private IAnimationUpdater animationUpdater;

        private Animation selectedAnimation = null;

        public Animation SelectedAnimation
        {
            set
            {
                if (SetValue(ref selectedAnimation, value))
                {
                    StopAnimation();

                    if (value != null)
                    {

                        switch (value.AnimationType)
                        {
                            case AnimationType.Keyframe:
                                animationUpdater = new NodeAnimationUpdater(value);
                                break;

                            default:
                                animationUpdater = new NodeAnimationUpdater(value);
                                break;
                        }
                        animationUpdater.RepeatMode = AnimationRepeatMode.Loop;
                    }
                    else
                    {
                        animationUpdater = null;
                    }

                    if (enableAnimation)
                    {
                        StartAnimation();
                    }
                }
            }
            get
            {
                return selectedAnimation;
            }

        }


        private AnimationRepeatMode selectedRepeatMode = AnimationRepeatMode.Loop;
        public AnimationRepeatMode SelectedRepeatMode
        {
            set
            {
                if (SetValue(ref selectedRepeatMode, value))
                {
                    reset = true;
                }
            }
            get { return selectedRepeatMode; }
        }


        public MainViewModel(MainWindow window)
        {
            this.mainWindow = window;
            this.Title = "My helix practise";
            this.SubTitle = "WPF & SharpDX";

            EffectsManager = new DefaultEffectsManager();

            /*
            Camera = new OrthographicCamera()
            {
                LookDirection = new System.Windows.Media.Media3D.Vector3D(0, -10, -10),
                Position = new System.Windows.Media.Media3D.Point3D(0, 10, 10),
                UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0),
                FarPlaneDistance = 5000,
                NearPlaneDistance = 0.1f
            };
            */

            /**/
            this.Camera = new PerspectiveCamera
            {
                Position = new Media3D.Point3D(50, 50, 50),
                LookDirection = new Media3D.Vector3D(-50, -50, -50),
                UpDirection = new Media3D.Vector3D(0, 1, 0),
                NearPlaneDistance = 1,
                FarPlaneDistance = 20000,
                FieldOfView = 15
            };


            InitBindingCommand();
        }


        private void CompositeHelper_Rendering(object sender, System.Windows.Media.RenderingEventArgs e)
        {
            if (animationUpdater != null)
            {
                if (reset)
                {
                    animationUpdater.Reset();
                    animationUpdater.RepeatMode = SelectedRepeatMode;
                    reset = false;
                }
                else
                {
                    animationUpdater.Update(Stopwatch.GetTimestamp(), Stopwatch.Frequency);
                }
            }
        }




        protected override void Dispose(bool disposing)
        {
            cts.Cancel(true);
            compositeHelper.Dispose();
            base.Dispose(disposing);
        }

    }
}
