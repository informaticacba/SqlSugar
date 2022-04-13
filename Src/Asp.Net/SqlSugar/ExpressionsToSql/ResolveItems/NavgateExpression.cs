﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar 
{
    internal class OneToOneNavgateExpression
    {
        private SqlSugarProvider context;
        private EntityInfo EntityInfo;
        private EntityInfo ProPertyEntity;
        private Navigat Navigat;
        public string ShorName;
        private string MemberName;
        public OneToOneNavgateExpression(SqlSugarProvider context)
        {
            this.context = context;
        }

        internal bool IsNavgate(Expression expression)
        {
            var exp = expression;
            if (exp is UnaryExpression) 
            {
                exp = (exp as UnaryExpression).Operand;
            }
            if (exp is MemberExpression) 
            {
                var memberExp=exp as MemberExpression;
                var childExpression = memberExp.Expression;
                if (childExpression != null && childExpression is MemberExpression) 
                {
                    var child2Expression = (childExpression as MemberExpression).Expression;
                    if (child2Expression.Type.IsClass()&& child2Expression is ParameterExpression) 
                    {
                       var entity= this.context.EntityMaintenance.GetEntityInfo(child2Expression.Type);
                        if (entity.Columns.Any(x => x.PropertyName == (childExpression as MemberExpression).Member.Name && x.Navigat != null)) 
                        {
                            EntityInfo = entity;
                            ShorName = child2Expression.ToString();
                            MemberName= memberExp.Member.Name;
                            ProPertyEntity = this.context.EntityMaintenance.GetEntityInfo(childExpression.Type);
                            Navigat = entity.Columns.FirstOrDefault(x => x.PropertyName == (childExpression as MemberExpression).Member.Name).Navigat;
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        internal MapperSql GetSql()
        {
            var pk = this.ProPertyEntity.Columns.First(it => it.IsPrimarykey == true).DbColumnName;
            var name = this.EntityInfo.Columns.First(it => it.PropertyName == Navigat.Name).DbColumnName;
            var selectName = this.ProPertyEntity.Columns.First(it => it.PropertyName ==MemberName).DbColumnName;
            MapperSql mapper = new MapperSql();
            var queryable = this.context.Queryable<object>();
            pk = queryable.QueryBuilder.Builder.GetTranslationColumnName(pk);
            name = queryable.QueryBuilder.Builder.GetTranslationColumnName(name);
            selectName = queryable.QueryBuilder.Builder.GetTranslationColumnName(selectName);
            mapper.Sql = queryable
                .AS(this.ProPertyEntity.DbTableName)
                .Where($" {ShorName}.{name}={pk} ").Select(selectName).ToSql().Key;
            mapper.Sql = $" ({mapper.Sql}) ";
            return mapper;
        }
    }
}