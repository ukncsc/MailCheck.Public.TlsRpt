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

namespace MailCheck.TlsRpt.EntityHistory.Dao {
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
    public class TlsRptEntityHistoryDaoResouces {
        
        private static ResourceManager resourceMan;
        
        private static CultureInfo resourceCulture;
        
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TlsRptEntityHistoryDaoResouces() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceManager ResourceManager {
            get {
                if (ReferenceEquals(resourceMan, null)) {
                    ResourceManager temp = new ResourceManager("MailCheck.TlsRpt.EntityHistory.Dao.TlsRptEntityHistoryDaoResouces", typeof(TlsRptEntityHistoryDaoResouces).Assembly);
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
        ///   Looks up a localized string similar to INSERT INTO `tls_rpt_entity_history`
        ///(`id`,
        ///`state`)
        ///VALUES
        ///(LOWER(@domain),
        ///@state)
        ///ON DUPLICATE KEY UPDATE
        ///state = @state.
        /// </summary>
        public static string InsertTlsRptEntityHistory {
            get {
                return ResourceManager.GetString("InsertTlsRptEntityHistory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT state 
        ///FROM tls_rpt_entity_history
        ///WHERE id = @domain;.
        /// </summary>
        public static string SelectTlsRptHistoryEntity {
            get {
                return ResourceManager.GetString("SelectTlsRptHistoryEntity", resourceCulture);
            }
        }
    }
}
