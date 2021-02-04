using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Assimp;
using HelixToolkit.Wpf.SharpDX.Model;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using Microsoft.Win32;
using MyHelixPractise.Base;
using MyHelixPractise.Data;

namespace MyHelixPractise
{
    using Media3D = System.Windows.Media.Media3D;

    public partial class MainViewModel
    {

        public const string SUPPORTED_3D_FILE_FILTER = "3D Files (*.FBX; *.OBJ; *.3DS)|*.fbx; *.obj; *.3ds|All files (*.*)|*.*";


        private void InitBindingCommand()
        {

            this.Command_ImportModel = new DelegateCommand(this.ImportModel);
            this.Command_ResetCamera = new DelegateCommand(this.ResetCamera);
            //this.Command_ResetCamera = new DelegateCommand(this.Command_Delete);
            //this.Command_ResetCamera = new DelegateCommand(this.Command_TranslateModel);
            //this.Command_ResetCamera = new DelegateCommand(this.Command_RotateModel);
            //this.Command_ResetCamera = new DelegateCommand(this.Command_ScaleModel);
            
        }

        public void DeleteModel() {

        }


        public ICommand Command_ImportModel { get; set; }


        public ICommand Command_ResetCamera { set; get; }


        public ICommand Command_DeleteModel { private set; get; }


        public ICommand Command_TranslateModel { private set; get; }


        public ICommand Command_RotateModel { private set; get; }
        
        
        public ICommand Command_ScaleModel{ private set; get; }



        public void ImportModel()
        {

            if (IsLoading)
                return;

            string selectedFilePath = OpenFileDialogWithFilter(MainViewModel.SUPPORTED_3D_FILE_FILTER);

            if (selectedFilePath == null)
                return;

            //TODO StopAnimation here

            IsLoading = true;
            Task.Run(() =>
            {
                Importer importer = new Importer();

                //true로 설정하면 뼈대가 보인다
                importer.Configuration.CreateSkeletonForBoneSkinningMesh = true;
                //importer.Configuration.SkeletonSizeScale = 0.04f;
                //importer.Configuration.GlobalScale = 0.1f;

                //Importer의 Load 메소드는 선택된 파일의 유효성 (호환성)을 검사한다.
                //내부에서 BuildScene이라는 Node를 조립하는 메소드를 호출한다.
                return importer.Load(selectedFilePath);
            }).ContinueWith((resultScene) =>
            {

                IsLoading = false;

                if (resultScene.IsCompleted)
                {
                    scene = resultScene.Result;
                    Animations.Clear();
                    GroupModel.Clear();

                    if (scene != null)
                    {

                        GroupModel.AddNode(scene.Root);

                        if (scene.HasAnimation)
                        {
                            foreach (var ani in scene.Animations)
                            {
                                Animations.Add(ani);
                            }
                        }

                        if (scene.Root != null)
                        {
                            foreach (var node in scene.Root.Traverse())
                            {


                                if (node is BoneSkinMeshNode boneNode)
                                {

                                    if (boneNode.IsSkeletonNode)
                                    {
                                        skeletonNodes.Add(boneNode);
                                        boneNode.Visible = false;
                                    }
                                    else
                                    {
                                        boneNode.IsThrowingShadow = true;
                                        boneNode.WireframeColor = new SharpDX.Color4(0, 0, 1, 1);
                                        boneSkinNodes.Add(boneNode);
                                        //boneNode.MouseDown += OnMouseDown_Model;
                                    }
                                }

                                node.Tag = new CustomDataModel(node);

                            }

                        }
                    }
                }
                else if (resultScene.IsFaulted && resultScene.Exception != null)
                {
                    MessageBox.Show(resultScene.Exception.Message);
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());

            return;
        }


        private string OpenFileDialogWithFilter(string filter)
        {
            var d = new OpenFileDialog();
            d.CustomPlaces.Clear();

            d.Filter = filter;

            if (!d.ShowDialog().Value)
            {
                return null;
            }

            return d.FileName;
        }


        private void ResetCamera()
        {

            if (Camera is OrthographicCamera)
            {
                (Camera as OrthographicCamera).Reset();
                (Camera as OrthographicCamera).FarPlaneDistance = 5000;
                (Camera as OrthographicCamera).NearPlaneDistance = 0.1f;
            }
            else if (Camera is PerspectiveCamera)
            {
                (Camera as PerspectiveCamera).Reset();
                (Camera as PerspectiveCamera).FarPlaneDistance = 20000;
                (Camera as PerspectiveCamera).NearPlaneDistance = 1f;
            }
            else
            {

            }

        }



    }
}
