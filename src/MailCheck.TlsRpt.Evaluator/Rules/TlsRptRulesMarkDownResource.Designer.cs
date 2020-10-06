﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace MailCheck.TlsRpt.Evaluator.Rules {
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [DebuggerNonUserCode()]
    [CompilerGenerated()]
    public class TlsRptRulesMarkDownResource {
        
        private static ResourceManager resourceMan;
        
        private static CultureInfo resourceCulture;
        
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TlsRptRulesMarkDownResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceManager ResourceManager {
            get {
                if (ReferenceEquals(resourceMan, null)) {
                    ResourceManager temp = new ResourceManager("MailCheck.TlsRpt.Evaluator.Rules.TlsRptRulesMarkDownResource", typeof(TlsRptRulesMarkDownResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your TLS-RPT record contains the wrong email address for Mail Check aggregate report processing.
        ///
        ///Please change your TLS-RPT record to be:
        ///
        ///`{0}`.
        /// </summary>
        public static string RuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage {
            get {
                return ResourceManager.GetString("RuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The TLS-RPT record does not contain the NCSC Mail Check email address, it&apos;s fine to use other tools but be aware that we won&apos;t be able to help you investigate encrypted email deliverability, and you won&apos;t see any reporting in Mail Check.
        ///
        ///If you would like Mail Check to receive a copy of your reports, then please change your record to:
        ///
        ///`{0}`.
        /// </summary>
        public static string RuaTagsShouldContainTlsRptServiceMailBoxErrorMessage {
            get {
                return ResourceManager.GetString("RuaTagsShouldContainTlsRptServiceMailBoxErrorMessage", resourceCulture);
            }
        }
    }
}