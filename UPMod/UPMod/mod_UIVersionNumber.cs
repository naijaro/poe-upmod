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
        [ModifiesMember("UpdateText")]
        private void mod_UpdateText()
        {
            if (this.m_Label)
            {
                // ORIGINAL (2026):
                // this.m_Label.text = ProductConfiguration.GetVersion();

                // PATCH:
                // append UPMod version while preserving game's full version format
                // example:
                // 3.9.4.12345 - UPMod 1.01.394
                // - where 1.01 is mod version
                // - and .393 is the version of the game it is compatible with

                string gameVersion = ProductConfiguration.GetVersion();

                // UPMod version should track latest supported game major/minor/patch
                string upmodVersion = "1.01.394";

                this.m_Label.text =
                    string.Format(
                        "{0} - UPMod {1}",
                        gameVersion,
                        upmodVersion);
            }
        }
    }
}