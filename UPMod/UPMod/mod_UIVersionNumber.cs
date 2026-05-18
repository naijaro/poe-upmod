using System.Collections.Generic;
using Patchwork.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Reflection;
using System.Text;

namespace UPMod
{
    [ModifiesType]
    public class mod_UIVersionNumber : UIVersionNumber
    {

        [MemberAlias(".ctor", typeof(MonoBehaviour))]
        public void alias_MonoBehavior_ctor()
        {

        }

        [ModifiesMember(".ctor")]
        public void mod_ctor()
        {
            alias_MonoBehavior_ctor();
            var ieModVersion = "1.01.306";
            this.FormatString = $"v{{0}}.{{1}}.{{2}} {{3}} - UPMod {ieModVersion}";
            this.m_stringBuilder = new StringBuilder();
        }
    
    //[NewMember(null)]
    //public String GetIEModVersion()
    //{
    //    Type IEModType = System.Reflection.Assembly.GetExecutingAssembly().GetType("IEMod.IEModVersion", false);
    //    String version = typeof(UIVersionNumber).GetField("Version").GetValue(null).Dump();

    //    if (IEModType != null)
    //    {
    //        FieldInfo[] fields = IEModType.GetFields();

    //        for (int i = 0; i < fields.Length; i++)
    //        {
    //            bool isVersion = "Version".Equals(fields[i].Name.ToString(), System.StringComparison.OrdinalIgnoreCase);

    //            if (isVersion)
    //            {
    //                object vObj = (String)fields[i].GetValue();
    //                return fields[i].GetValue.ToString();
    //            }
    //        }
    //    }

    //    return null;
    //}
    }
}