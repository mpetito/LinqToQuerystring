namespace LinqToQuerystring.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    using Antlr.Runtime;

    using LinqToQuerystring.TreeNodes;
    using LinqToQuerystring.TreeNodes.Base;

    public class ExpandNode : QueryModifier
    {
        public ExpandNode(Type inputType, IToken payload, TreeNodeFactory treeNodeFactory)
            : base(inputType, payload, treeNodeFactory)
        {
        }

        public override IQueryable ModifyQuery(IQueryable query)
        {
            foreach (var child in this.ChildNodes)
            {
                var parameter = Expression.Parameter(this.inputType, "o");
                var childExpression = child.BuildLinqExpression(query, query.Expression, parameter);

                var path = GetMemberPath(childExpression, parameter);
                query = query.Include(path);
            }

            return query;
        }

        static string GetMemberPath(Expression expr, ParameterExpression param)
        {
            if (expr == param) return null;

            var me = expr as MemberExpression;
            if (me != null)
            {
                var root = GetMemberPath(me.Expression, param);

                return root == null ? me.Member.Name : string.Concat(root, ".", me.Member.Name);
            }

            var ue = expr as UnaryExpression;
            if (ue != null)
            {
                return GetMemberPath(ue.Operand, param);
            }

            return null;
        }
    }
}