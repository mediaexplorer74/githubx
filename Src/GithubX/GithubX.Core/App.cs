using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.IoC;
using System;
using System.Diagnostics;

namespace GithubXamarin.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();

            try
            {
                CreatableTypes()
                    .EndingWith("Repository")
                    .AsInterfaces()
                    .RegisterAsLazySingleton();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] CreatableTypes exception: " + ex);
            }

            try
            {
                CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] CreatableTypes exception: " + ex);
            }

            RegisterAppStart(new AppStart());
        }
    }
}
