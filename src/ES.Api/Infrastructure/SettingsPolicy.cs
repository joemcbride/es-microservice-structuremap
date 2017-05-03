using System;
using Baseline;
using Microsoft.Extensions.Configuration;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace ES.Api
{
    /// <summary>
    /// Policy to create settings objects bound with values from appsettings.json.
    /// </summary>
    public class SettingsPolicy : IFamilyPolicy
    {
        public bool AppliesToHasFamilyChecks => true;

        public PluginFamily Build(Type type)
        {
            if (type.Name.EndsWith("Settings") && type.IsConcreteWithDefaultCtor())
            {
                var family = new PluginFamily(type);
                var instance = getDefaultInstance(type);
                family.SetDefault(instance);
                return family;
            }
            return null;
        }

        private Instance getDefaultInstance(Type type)
        {
            var instanceType = typeof(SettingsInstance<>).MakeGenericType(type);
            return Activator.CreateInstance(instanceType).As<Instance>();
        }
    }

    public class SettingsInstance<T> : LambdaInstance<T> where T : class, new()
    {
        public SettingsInstance()
            : base($"Building {typeof(T).FullName} from settings file",
                  c => c.GetInstance<IConfigurationRoot>().Get<T>())
        {
        }
    }
}
