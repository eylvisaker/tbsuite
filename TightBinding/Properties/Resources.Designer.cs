﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4200
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TightBindingSuite.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TightBindingSuite.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /001/
        /// 0:   (+X  ,+Y  ,+Z  ) + (0   ,0   ,0   )  :   E
        ////002/
        ///32:   (-X  ,-Y  ,-Z  ) + (0   ,0   ,0   )  :   I
        ////003/
        /// 1:   (-X  ,+Y  ,-Z  ) + (0   ,0   ,0   )  :   C2(y)
        ////004/
        /// 1:   (-X  ,+Y  ,-Z  ) + (0   ,1/2 ,0   )  :   C2(y)
        ////005/
        /// 1:   (-X  ,+Y  ,-Z  ) + (0   ,0   ,0   )  :   C2(y)
        ////006/
        ///33:   (+X  ,-Y  ,+Z  ) + (0   ,0   ,0   )  :   s(y)
        ////007/
        ///33:   (+X  ,-Y  ,+Z  ) + (0   ,0   ,1/2 )  :   s(y)
        ////008/
        ///33:   (+X  ,-Y  ,+Z  ) + (0   ,0   ,0   )  :   s(y)
        ////009/
        ///33:   (+X  ,-Y  ,+Z  ) + (0   ,0   ,1/2 )  :  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string spgroup {
            get {
                return ResourceManager.GetString("spgroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 1 - P1
        ///10 - P2/M
        ///100 - P4BM
        ///101 - P42CM
        ///102 - P42NM
        ///103 - P4CC
        ///104 - P4NC
        ///105 - P42MC
        ///106 - P42BC
        ///107 - I4MM
        ///108 - I4CM
        ///109 - I41MD
        ///11 - P21/M
        ///110 - I41CD
        ///111 - PB42M
        ///112 - PB42C
        ///113 - PB421M
        ///114 - PB421C
        ///115 - PB4M2
        ///116 - PB4C2
        ///117 - PB4B2
        ///118 - PB4N2
        ///119 - IB4M2
        ///12 - C2/M
        ///120 - IB4C2
        ///121 - IB42M
        ///122 - IB42D
        ///123 - P4/MMM
        ///124 - P4/MCC
        ///125 - P4/NBM
        ///126 - P4/NNC
        ///127 - P4/MBM
        ///128 - P4/MNC
        ///129 - P4/NMM
        ///13 - P2/C
        ///130 - P4/NCC
        ///131 - P42/MMC
        ///132 - P42/MCM
        ///133 - P42/NBC
        ///134 - P42/NNM
        ///135 - P42/MBC
        ///136 - P42/MNM
        ///137 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string spnames {
            get {
                return ResourceManager.GetString("spnames", resourceCulture);
            }
        }
    }
}
