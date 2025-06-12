using System;
using System.Reflection;

namespace rupbes.Classes
{
    public class PropertyUpdater
    {
        public static void UpdateProperties<T>(T obj1, T obj2)
        {
            // Получаем все свойства типа T
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                // Проверяем, является ли свойство виртуальным
                if (property.GetGetMethod()?.IsVirtual == true)
                {
                    continue; // Пропускаем виртуальные свойства
                }

                // Получаем значения свойств
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                // Сравниваем значения и обновляем, если они различаются
                if (!object.Equals(value1, value2))
                {
                    property.SetValue(obj1, value2);
                }
            }
        }
    }
}