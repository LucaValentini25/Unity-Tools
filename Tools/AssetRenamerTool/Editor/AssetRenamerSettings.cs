using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.ClasterTools.AssetRenamer.Editor
{
    public class AssetRenamerSettings : ScriptableObject
    {
        public List<PrefixEntry> prefixEntries = new List<PrefixEntry>()
        {
            new PrefixEntry { categoryName = "Animation",         extension = ".anim",  prefix = "Anim" },
        new PrefixEntry { categoryName = "Animator Controller", extension = ".controller", prefix = "AnimCon" },
        new PrefixEntry { categoryName = "Audio",             extension = ".wav",   prefix = "Sound" },
        new PrefixEntry { categoryName = "Audio",             extension = ".mp3",   prefix = "Sound" },
        new PrefixEntry { categoryName = "Audio",             extension = ".ogg",   prefix = "Sound" },
        new PrefixEntry { categoryName = "Material",          extension = ".mat",   prefix = "Mat" },
        new PrefixEntry { categoryName = "Mesh",              extension = ".fbx",   prefix = "Mesh" },
        new PrefixEntry { categoryName = "Mesh",              extension = ".obj",   prefix = "Mesh" },
        new PrefixEntry { categoryName = "Prefab",            extension = ".prefab",prefix = "Prefab" },
        new PrefixEntry { categoryName = "Shader",            extension = ".shader",prefix = "Shad" },
        new PrefixEntry { categoryName = "Shader Graph",      extension = ".shadergraph", prefix = "SH_Graph" },
        new PrefixEntry { categoryName = "Texture",           extension = ".png",   prefix = "Tex" },
        new PrefixEntry { categoryName = "Texture",           extension = ".jpg",   prefix = "Tex" },
        new PrefixEntry { categoryName = "Texture",           extension = ".jpeg",  prefix = "Tex" },
        new PrefixEntry { categoryName = "Texture",           extension = ".tga",   prefix = "Tex" },
        new PrefixEntry { categoryName = "Texture",           extension = ".psd",   prefix = "Tex" },
        new PrefixEntry { categoryName = "ScriptableObject",  extension = ".asset", prefix = "SO" },
        new PrefixEntry { categoryName = "Timeline",          extension = ".playable", prefix = "Timeline" },
        new PrefixEntry { categoryName = "Sprite Atlas",      extension = ".spriteatlas", prefix = "Sprite_Atl" },
        new PrefixEntry { categoryName = "Lighting Settings", extension = ".lighting", prefix = "Light" },
        new PrefixEntry { categoryName = "Render Texture",    extension = ".renderTexture", prefix = "Rend_Tex" },
        };

        public string GetPrefixForExtension(string ext, out string categoryName)
        {
            foreach (var entry in prefixEntries)
            {
                if (entry.extension == ext)
                {
                    categoryName = entry.categoryName;
                    return entry.prefix;
                }
            }

            categoryName = "Unknown";
            return null;
        }
    } 
}