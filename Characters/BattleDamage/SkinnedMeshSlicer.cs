using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

///https://github.com/toninhoPinto/MeshSlicingRunTime
namespace ThunderPulse.Characters.BattleDamage
{
    public class Slice
    {
        Vector3 position;
        Vector3 normal;
    }

    /// <summary>
    /// Разрезатель моделей персонажей.
    /// </summary>
    public static class SkinnedMeshSlicer
    {
        /// <summary>
        /// Разрезаем модель, разрезаем скелет, создаем новые составные скелеты.
        /// </summary>
        /// <param name="characterMesh"></param>
        /// <param name="slice"></param>
        public static void Cut(SkinnedMeshRenderer characterMesh, Slice slice)
        {
            CutBones(characterMesh.bones, slice);
            ///TODO:
            ///определяем какие кости были разделены
            ///полностью копируем разделенные кости
            ///меняем иерархию костей, перемещаем(transform.parent) кости после обрубка в новосозданную кость
            ///выбираем вертексы меша - только те, что были привязаны к разрубленным костям
            ///разбиваем их на группы - до и после среза.
            ///привязываем вертексы после среза - ново созданной кости обрубка.
            ///вешаем RaggDoll-компоненты
        }

        /// <summary>
        /// Режем кости, определяем количество и состав новых скелетов.
        /// </summary>
        private static void CutBones(Transform[] bones, Slice slice)
        {
            foreach (Transform tr in bones)
            {

            }
        }

        /// <summary>
        /// Разделяем вертексы н
        /// </summary>
        /// <param name="characterMesh"></param>
        /// <param name="slice"></param>
        private static void Vertices(SkinnedMeshRenderer characterMesh, Slice slice)
        {

        }
    }
}
