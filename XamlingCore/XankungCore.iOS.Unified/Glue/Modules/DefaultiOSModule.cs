using Autofac;
using Foundation;
using XamlingCore.iOS.Unified.Implementations;
using XamlingCore.iOS.Unified.Implementations.Helpers;
using XamlingCore.iOS.Unified.Navigation;
using XamlingCore.iOS.Unified.Root;
using XamlingCore.iOS.Unified.Services;
using XamlingCore.Portable.Contract.Device.Service;
using XamlingCore.Portable.Contract.Downloaders;
using XamlingCore.Portable.Contract.Infrastructure.LocalStorage;
using XamlingCore.Portable.Contract.Network;
using XamlingCore.Portable.Contract.Services;
using XamlingCore.Portable.Contract.UI;

namespace XamlingCore.iOS.Unified.Glue.Modules
{
    public class DefaultiOSModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RootViewController>().AsSelf().SingleInstance();

            builder.RegisterType<LocalStorage>().As<ILocalStorage>().SingleInstance();
            builder.RegisterType<LoadStatusService>().As<ILoadStatusService>().SingleInstance();
            builder.RegisterType<EnvironmentService>().As<IEnvironmentService>().SingleInstance();
            builder.Register(_ => new iOSDispatcher(new NSObject())).As<IDispatcher>().SingleInstance();
            builder.RegisterType<LocationTrackingSensor>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DeviceNetworkStatus>().As<IDeviceNetworkStatus>().SingleInstance();

            builder.RegisterType<iOSViewResolver>().AsImplementedInterfaces().SingleInstance();
            
            builder.RegisterType<HashHelper>().AsImplementedInterfaces();

            builder.RegisterType<MotionSensor>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<OrientationSensor>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<iOSNativeHttpClientTransferrer>().As<IHttpTransferrer>().SingleInstance();
            builder.RegisterType<iOSSimpleNativeHttpHttpTransfer>().As<ISimpleHttpTranferrer>().SingleInstance();

            builder.RegisterType<DeviceUtilityService>().As<IDeviceUtilityService>().SingleInstance();
            base.Load(builder);
        }
    }
}