﻿using System;
using System.Linq;

namespace DeanZhou.Framework
{
    /// <summary>
    /// 简单过滤器
    /// 过滤单个元素（含参）
    /// </summary>
    /// <typeparam name="TItem">对象识别枚举数据类型</typeparam>
    /// <typeparam name="TParam">辅助参数类型</typeparam>
    public sealed class ElementFilterCore<TItem, TParam>
        where TItem : class
        where TParam : class
    {
        /// <summary>
        /// 自定义过滤器
        /// </summary>
        private Func<TItem, TParam, bool> Filters { get; set; }

        /// <summary>
        /// 检测当前政策行是否需要添加
        /// </summary>
        /// <param name="it"></param>
        /// <param name="pt"></param>
        public bool DoFilter(TItem it, TParam pt)
        {
            return Filters == null || Filters.GetInvocationList().All(x => x.DynamicInvoke(it, pt).ChangeType<bool>());
        }

        /// <summary>
        /// 添加自定义过滤器
        /// </summary>
        public ElementFilterCore<TItem, TParam> AddFilter(params Func<TItem, TParam, bool>[] filters)
        {
            if (filters == null)
            {
                Filters = null;
                return this;
            }
            foreach (Func<TItem, TParam, bool> filter in filters)
            {
                if (Filters == null)
                {
                    Filters = filter;
                }
                else
                {
                    Filters += filter;
                }
            }
            return this;
        }

        /// <summary>
        /// 添加自定义过滤器
        /// </summary>
        /// <param name="filters"></param>
        public ElementFilterCore<TItem, TParam> AddFilter(params IFilter<TItem, TParam>[] filters)
        {
            if (filters == null)
            {
                return this;
            }
            foreach (var filter in filters)
            {
                AddFilter(filter.DoFilter);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="filterFullClassNames"></param>
        /// <returns></returns>
        public ElementFilterCore<TItem, TParam> AddFilter(string assemblyName, params string[] filterFullClassNames)
        {
            if (filterFullClassNames == null)
            {
                return this;
            }
            foreach (var fullClassName in filterFullClassNames)
            {
                var instance = Common.CreateIFilter<TItem, TParam>(assemblyName, fullClassName);
                AddFilter(instance);
            }
            return this;
        }

    }

}
