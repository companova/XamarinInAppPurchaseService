using System;
using System.ComponentModel;

namespace Companova.Xamarin.InAppPurchase.Service
{
    // Copied: from iOS Implementation
    // Summary:
    //     Prevents the MonoTouch linker from linking the target.
    //
    // Remarks:
    //     This attribute is used at link time by the MonoTouch linker to skip certain classes,
    //     structures, enumerations or other objects from being linked.
    //     By applying this attribute all of the members of the target will be kept as if
    //     they had been referenced by the code.
    //     This attribute is useful for example when using classes that use reflection (for
    //     example web services) and that use this information for serialization and deserialization.
    //     Starting with MonoTouch 6.0.9 this attribute can also be used at the assembly
    //     level, effectively duplicating the same behaviour as --linkskip=ASSEMBLY but
    //     without the need to duplicate the extra argument to every project.
    //     You do not actually need to take a dependency on the Xamarin assemblies, for
    //     example, if you are a third-party developer that is creating a component or nuget
    //     package that is safe to be linked, you can just include the LinkerSafe attribute
    //     source code in your application, and the Xamarin linker will recognize it.
    //     To use in an assembly, without taking a dependency in Xamarin's assemblies:
    [AttributeUsage(AttributeTargets.All)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    sealed class PreserveAttribute : Attribute
    {
        public bool AllMembers;
        public bool Conditional;

        public PreserveAttribute(bool allMembers, bool conditional)
        {
            AllMembers = allMembers;
            Conditional = conditional;
        }

        /// <summary>
        /// Summary:
        ///     Instruct the MonoTouch linker to preserve the decorated code
        /// Remarks:
        ///     By default the linker, when enabled, will remove all the code that is not directly used by the application.
        /// </summary>
        public PreserveAttribute()
        {
        }
    }
}
