using Grasshopper.Kernel;
using Pancake.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pancake.Modules.PortabilityChecker
{
    public struct ResultEntry
    {
        public enum Type
        {
            Unset,
            Object,
            File,
            Information
        }

        public Type ResultType;
        public string Section;
        public string Name;
        public string SubNameOverride;
        public string Description;
        public bool AlwaysExpand;

        public Guid AssociatedObject;
        public string AssociatedFile;
        public static ResultEntry FromDesc(string section, string name, string description = null)
        {
            return new ResultEntry()
            {
                ResultType = Type.Information,
                Section = section,
                Name = name,
                SubNameOverride = null,
                Description = description,
            };
        }

        public static ResultEntry FromObject(string section, IGH_DocumentObject obj, string description = null)
        {
            return new ResultEntry()
            {
                ResultType = Type.Object,
                Section = section,
                Name = obj.Name,
                SubNameOverride = obj.Attributes?.Pivot.ToString() ?? obj.Name,
                Description = description,
                AssociatedObject = obj.InstanceGuid
            };
        }

        public static ResultEntry FromObject(string section, string name, IGH_DocumentObject obj, string description = null)
        {
            return new ResultEntry()
            {
                ResultType = Type.Object,
                AlwaysExpand = true,
                Section = section,
                Name = name,
                SubNameOverride = obj.Name + " @" + obj.Attributes?.Pivot.ToString(),
                Description = description,
                AssociatedObject = obj.InstanceGuid
            };
        }

        public static ResultEntry FromFile(string section, string file, string description = null)
        {
            return new ResultEntry()
            {
                ResultType = Type.File,
                Section = section,
                Name = Path.GetFileName(file),
                AssociatedFile = file,
                Description = !string.IsNullOrEmpty(description) ? $"{description}\r\n\r\n{file}" : file
            };
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Description))
            {
                if (!string.IsNullOrEmpty(AssociatedFile))
                {
                    return AssociatedFile;
                }

                if (AssociatedObject != Guid.Empty)
                {
                    return AssociatedObject.ToString();
                }

                return "";
            }
            else
            {
                if (!string.IsNullOrEmpty(AssociatedFile))
                {
                    return Description + ", " + AssociatedFile;
                }

                return Description;
            }
        }
    }

    public abstract class StaticConfiguration : IPortabilityCheckerConfiguration
    {
        public abstract string Name { get; }

        public IEnumerable<Base> Checkers => _checkers;

        private List<Base> _checkers = new List<Base>();

        protected void Add<T>() where T : Base, new()
        {
            _checkers.Add(new T());
        }

        protected void Add(Base T)
        {
            _checkers.Add(T);
        }

        public virtual bool Hidden => false;
    }
}
