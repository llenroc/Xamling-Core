//using System;
//using System.Reflection;
//using Autofac;
//using UIKit;
//using Xamarin.Forms;
//using XamlingCore.iOS.Unified.Root;
//using XamlingCore.Portable.Contract.Glue;
//using XamlingCore.Portable.View.ViewModel;
//using XamlingCore.XamarinThings.Contract;
//using XamlingCore.XamarinThings.Core;
//using XamlingCore.XamarinThings.Frame;

//namespace XamlingCore.iOS.Unified
//{
//    /// <summary>
//    /// Boots the app to the initial frame
//    /// </summary>
//    /// <typeparam name="TRootFrame"></typeparam>
//    /// <typeparam name="TRootVM"></typeparam>
//    /// <typeparam name="TInitialVM"></typeparam>
//    /// <typeparam name="TGlue"></typeparam>
//    public class XiOSCore<TRootFrame, TRootViewModel, TGlue> : XCore<TRootFrame, TGlue>
//        where TRootFrame : XFrame
//        where TRootViewModel : XViewModel
//        where TGlue : class, IGlue, new()
//    {
        

//        private UIWindow _window;
//        private UIViewController _rootView;
        

//        public UIViewController RootView
//        {
//            get { return _rootView; }
//            set { _rootView = value; }
//        }

        

//        public void Init()
//        {
//            XCorePlatform.Platform = XCorePlatform.XCorePlatforms.iOS;

//            InitRoot(); 

            

            
//            _window = new UIWindow(UIScreen.MainScreen.Bounds);

//            var rv = RootFrame.Container.Resolve<RootViewController>();
//            var childView = initalViewController.CreateViewController();

//            rv.SetChild(childView, _window);
          
//            RootView = rv;
            
//            XiOSRoot.RootViewController = RootView;
//            XiOSRoot.RootWindow = _window;
//            _window.RootViewController = RootView;

//            _window.MakeKeyAndVisible();
//        }

        

     
//        public override void ShowNativeView(string viewName)
//        {
//            if (viewName.ToLower().IndexOf("storyboard") != -1)
//            {
//                RootViewModel.Dispatcher.Invoke(() =>
//                {
//                    var sb = UIStoryboard.FromName(viewName, null);
//                    var controller = sb.InstantiateInitialViewController() as UIViewController;
//                    RootView.PresentViewController(controller, false, null);
//                });
//                return;
//            }


//            //must be in the main assembly
//            var a = Assembly.GetEntryAssembly();

//            var t = a.GetType(viewName);

//            //is it regsitered?

//            if (!Container.IsRegistered(t))
//            {
//                throw new Exception("Could not find native view: " + t.FullName);
//            }

//            RootViewModel.Dispatcher.Invoke(() =>
//            {
//                var controller = Container.Resolve(t) as UIViewController;

//                if (controller == null)
//                {
//                    throw new Exception("Could not resolve navtive view as UIViewController: " + t.FullName);
//                }
                
//                RootView.PresentViewController(controller, false, null);
//            });



//        }
//    }

//}