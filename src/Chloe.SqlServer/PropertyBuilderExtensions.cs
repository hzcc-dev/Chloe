﻿using Chloe.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.SqlServer
{
    public static class PrimitivePropertyBuilderExtension
    {
        public static IPrimitivePropertyBuilder HasSqlServerDataType(this IPrimitivePropertyBuilder propertyBuilder, string name)
        {
            DataTypeAttribute dataTypeAttribute = new DataTypeAttribute(name);
            propertyBuilder.HasAnnotation(dataTypeAttribute);
            return propertyBuilder;
        }

        public static IPrimitivePropertyBuilder<TProperty, TEntity> HasSqlServerDataType<TProperty, TEntity>(this IPrimitivePropertyBuilder<TProperty, TEntity> propertyBuilder, string name)
        {
            (propertyBuilder as IPrimitivePropertyBuilder).HasSqlServerDataType(name);
            return propertyBuilder;
        }
    }
}
