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

namespace MailCheck.TlsRpt.Entity.Dao {
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
    public class TlsRptEntityDaoResouces {
        
        private static ResourceManager resourceMan;
        
        private static CultureInfo resourceCulture;
        
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TlsRptEntityDaoResouces() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceManager ResourceManager {
            get {
                if (ReferenceEquals(resourceMan, null)) {
                    ResourceManager temp = new ResourceManager("MailCheck.TlsRpt.Entity.Dao.TlsRptEntityDaoResouces", typeof(TlsRptEntityDaoResouces).Assembly);
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
        ///   Looks up a localized string similar to DELETE FROM tls_rpt_entity WHERE id = @id;.
        /// </summary>
        public static string DeleteTlsRptEntity {
            get {
                return ResourceManager.GetString("DeleteTlsRptEntity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO `tls_rpt_entity`
        ///(`id`,
        ///`version`,
        ///`state`)
        ///VALUES
        ///(LOWER(@domain),
        ///@version,
        ///@state)
        ///ON DUPLICATE KEY UPDATE 
        ///state = IF(version &lt; @version, VALUES(state), state),
        ///version = IF(version &lt; @version, VALUES(version), version);.
        /// </summary>
        public static string InsertTlsRptEntity {
            get {
                return ResourceManager.GetString("InsertTlsRptEntity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT state 
        ///FROM tls_rpt_entity
        ///WHERE id = @domain
        ///ORDER BY version DESC
        ///LIMIT 1;.
        /// </summary>
        public static string SelectTlsRptEntity {
            get {
                return ResourceManager.GetString("SelectTlsRptEntity", resourceCulture);
            }
        }
    }
}
