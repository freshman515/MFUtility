using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MFUtility.Mvvm.Wpf.Framework.Bases {
    public class BaseViewModel : INotifyPropertyChanged {
        // PropertyChanged 事件
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并通知
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否属性值发生变化</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 批量通知属性更新（多属性）
        /// </summary>
        /// <param name="propertyNames">多个属性名称</param>
        protected void OnPropertiesChanged(params string[] propertyNames) {
            foreach (var propertyName in propertyNames.Distinct()) {
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// 设置多个属性，并通知变化
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="fields">字段与新值的集合</param>
        /// <returns>是否发生变化</returns>
        protected bool SetProperties<T>(Dictionary<string, T> fields) {
            bool isChanged = false;

            foreach (var field in fields) {
                var fieldName = field.Key;
                var value = field.Value;

                var currentValue = GetFieldValue<T>(fieldName);
                if (!EqualityComparer<T>.Default.Equals(currentValue, value)) {
                    SetProperty(ref currentValue, value, fieldName);
                    isChanged = true;
                }
            }

            return isChanged;
        }

        /// <summary>
        /// 获取字段的值（通过反射获取属性字段的值）
        /// </summary>
        /// <typeparam name="T">字段类型</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <returns>字段值</returns>
        private T GetFieldValue<T>(string propertyName) {
            var field = GetType().GetField(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null) throw new InvalidOperationException($"字段 '{propertyName}' 未找到！");
            return (T)field.GetValue(this);
        }
    }
}
