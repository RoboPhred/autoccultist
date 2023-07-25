namespace AutoccultistNS.Config
{
    using System;
    using AutoccultistNS.UI;
    using UnityEngine;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class UISettingsConfig : ConfigObject, IYamlConvertible
    {
        private string icon = null;

        public UISettingsConfig()
        {
        }

        public bool Visible { get; set; } = true;

        public string HomeSituation { get; set; } = null;

        public string Icon
        {
            get
            {
                if (!string.IsNullOrEmpty(this.icon))
                {
                    return this.icon;
                }

                if (!string.IsNullOrEmpty(this.HomeSituation))
                {
                    return $"verb:{this.HomeSituation}";
                }

                return null;
            }

            set
            {
                this.icon = value;
            }
        }

        public Sprite GetIcon()
        {
            if (string.IsNullOrEmpty(this.Icon))
            {
                return null;
            }

            return ResourceResolver.GetSprite(this.Icon);
        }

        public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            if (parser.TryConsume<Scalar>(out var scalar))
            {
                if (scalar.Value == "false")
                {
                    this.Visible = false;
                }
                else if (scalar.Value == "true")
                {
                    this.Visible = true;
                }
                else
                {
                    this.Visible = true;
                    this.HomeSituation = scalar.Value;
                }
            }
            else
            {
                var obj = (UISettingsConfigObject)nestedObjectDeserializer(typeof(UISettingsConfigObject));
                this.Visible = obj.Visible;
                this.HomeSituation = obj.HomeSituation;
                this.Icon = obj.Icon;
            }
        }

        public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            throw new NotSupportedException();
        }

        private class UISettingsConfigObject
        {
            public bool Visible { get; set; } = true;

            public string HomeSituation { get; set; } = null;

            public string Icon { get; set; } = null;
        }
    }
}
