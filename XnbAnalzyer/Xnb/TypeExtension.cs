﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalzyer.Xnb
{
    public static class TypeExtension
    {
        public static string GetFullName(this Type type)
        {
            if (type.FullName is not null)
            {
                return type.FullName;
            }

            if (type.Namespace is not null)
            {
                return $"{type.Namespace}.{type.Name}";
            }

            return type.Name;
        }
    }
}
