using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace MFUtility.UI
{
    /// <summary>
    /// 🧩 EnumComboBox — 自动绑定枚举项的 ComboBox。
    /// 支持 Description、Display、Lang、自定义特性（任意属性名）。
    /// 可用 EnumType 或 EnumSource 动态切换。
    /// </summary>
    public class EnumComboBox : ComboBox
    {
        static EnumComboBox() { }

        public EnumComboBox()
        {
            Loaded += (_, _) => RefreshItems();
        }

        #region === 依赖属性 ===

        /// <summary>绑定枚举类型（例如 {x:Type local:UserRole} 或 {Binding CurrentEnumType}）</summary>
        public Type EnumType
        {
            get => (Type)GetValue(EnumTypeProperty);
            set => SetValue(EnumTypeProperty, value);
        }
        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register(nameof(EnumType), typeof(Type), typeof(EnumComboBox),
                new PropertyMetadata(null, OnEnumTypeChanged));

        /// <summary>绑定当前选中项</summary>
        public object SelectedEnumValue
        {
            get => GetValue(SelectedEnumValueProperty);
            set => SetValue(SelectedEnumValueProperty, value);
        }
        public static readonly DependencyProperty SelectedEnumValueProperty =
            DependencyProperty.Register(nameof(SelectedEnumValue), typeof(object), typeof(EnumComboBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedEnumChanged));

        /// <summary>显示内容来源（Description / Display / Lang / Value / Name / 任意Attribute名）</summary>
        public string DisplayMember
        {
            get => (string)GetValue(DisplayMemberProperty);
            set => SetValue(DisplayMemberProperty, value);
        }
        public static readonly DependencyProperty DisplayMemberProperty =
            DependencyProperty.Register(nameof(DisplayMember), typeof(string), typeof(EnumComboBox),
                new PropertyMetadata("Description", OnEnumTypeChanged));

        /// <summary>指定从特性中读取哪个属性（比如 En / Zh / Name）</summary>
        public string DisplayProperty
        {
            get => (string)GetValue(DisplayPropertyProperty);
            set => SetValue(DisplayPropertyProperty, value);
        }
        public static readonly DependencyProperty DisplayPropertyProperty =
            DependencyProperty.Register(nameof(DisplayProperty), typeof(string), typeof(EnumComboBox),
                new PropertyMetadata(null, OnEnumTypeChanged));

        /// <summary>语言模式（仅当 DisplayMember="Lang" 时有效）</summary>
        public string LangMode
        {
            get => (string)GetValue(LangModeProperty);
            set => SetValue(LangModeProperty, value);
        }
        public static readonly DependencyProperty LangModeProperty =
            DependencyProperty.Register(nameof(LangMode), typeof(string), typeof(EnumComboBox),
                new PropertyMetadata("Zh", OnEnumTypeChanged));

        /// <summary>自动根据系统语言选择中文/英文</summary>
        public bool AutoLangMode
        {
            get => (bool)GetValue(AutoLangModeProperty);
            set => SetValue(AutoLangModeProperty, value);
        }
        public static readonly DependencyProperty AutoLangModeProperty =
            DependencyProperty.Register(nameof(AutoLangMode), typeof(bool), typeof(EnumComboBox),
                new PropertyMetadata(true, OnEnumTypeChanged));

        #endregion

        #region === 刷新逻辑 ===

        private static void OnEnumTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EnumComboBox cb)
                cb.RefreshItems();
        }

        private static void OnSelectedEnumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EnumComboBox cb && e.NewValue != null)
                cb.SelectedValue = e.NewValue;
        }

        private void RefreshItems()
        {
            if (EnumType == null || !EnumType.IsEnum)
            {
                ItemsSource = null;
                return;
            }

            // 自动语言检测
            string lang = LangMode;
            if (AutoLangMode)
            {
                var uiLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                lang = uiLang switch
                {
                    "zh" => "Zh",
                    "en" => "En",
                    _ => "En"
                };
            }

            var list = new List<EnumItem>();
            foreach (var val in Enum.GetValues(EnumType))
            {
                var fi = EnumType.GetField(val.ToString()!);
                if (fi == null) continue;

                string display = val.ToString()!;
                if (DisplayMember.Equals("Description", StringComparison.OrdinalIgnoreCase))
                {
                    display = fi.GetCustomAttribute<DescriptionAttribute>()?.Description ?? display;
                }
                else if (DisplayMember.Equals("Display", StringComparison.OrdinalIgnoreCase))
                {
                    var displayAttr = fi.GetCustomAttributes()
                        .FirstOrDefault(a => a.GetType().Name == "DisplayAttribute");
                    if (displayAttr != null)
                    {
                        var nameProp = displayAttr.GetType().GetProperty("Name");
                        display = nameProp?.GetValue(displayAttr)?.ToString() ?? display;
                    }
                }
                else if (DisplayMember.Equals("Lang", StringComparison.OrdinalIgnoreCase))
                {
                    var langAttr = fi.GetCustomAttribute<LangAttribute>();
                    if (langAttr != null)
                        display = lang.Equals("En", StringComparison.OrdinalIgnoreCase) ? langAttr.En : langAttr.Zh;
                }
                else if (DisplayMember.Equals("Value", StringComparison.OrdinalIgnoreCase))
                {
                    display = Convert.ToInt32(val).ToString();
                }
                else if (!string.IsNullOrEmpty(DisplayMember))
                {
                    // 支持任意自定义特性
                    var attr = fi.GetCustomAttributes()
                        .FirstOrDefault(a =>
                            a.GetType().Name.Equals(DisplayMember + "Attribute", StringComparison.OrdinalIgnoreCase));

                    if (attr != null)
                    {
                        PropertyInfo? prop = null;
                        if (!string.IsNullOrEmpty(DisplayProperty))
                            prop = attr.GetType().GetProperty(DisplayProperty,
                                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        prop ??= attr.GetType().GetProperty("En") ??
                                 attr.GetType().GetProperty("Name") ??
                                 attr.GetType().GetProperties().FirstOrDefault(p => p.PropertyType == typeof(string));

                        if (prop != null)
                            display = prop.GetValue(attr)?.ToString() ?? display;
                    }
                }

                list.Add(new EnumItem { Value = val, Text = display });
            }

            ItemsSource = list;
            DisplayMemberPath = nameof(EnumItem.Text);
            SelectedValuePath = nameof(EnumItem.Value);

            if (SelectedEnumValue != null)
                SelectedValue = SelectedEnumValue;
        }

        #endregion

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (SelectedValue != null)
                SelectedEnumValue = SelectedValue;
        }

        private class EnumItem
        {
            public string Text { get; set; } = "";
            public object Value { get; set; } = default!;
        }
    }

    /// <summary>
    /// ✅ 多语言显示特性：[Lang("中文", "English")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LangAttribute : Attribute
    {
        public string Zh { get; }
        public string En { get; }

        public LangAttribute(string zh, string en)
        {
            Zh = zh;
            En = en;
        }
    }
}
