using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbGroupByCollection : IDbObject, IEnumerable<IDbSelectable>
    {
        private readonly List<IDbSelectable> _groupBys = new List<IDbSelectable>();

        public void Add(IDbSelectable selectable)
        {
            if (_groupBys.Contains(selectable, SqlSelectableComparerInstance))
                return;

            _groupBys.Add(selectable);
        }

        public bool IsSingleKey { get; set; }

        public override string ToString()
        {
            var groupbys = _groupBys.SelectMany(g =>
                (g as IDbRefColumn)?.GetPrimaryKeys()?.Cast<IDbSelectable>() ??
                new[] {g});
                           
            return string.Join(", ", groupbys);
        }

        public IEnumerator<IDbSelectable> GetEnumerator()
        {
            return _groupBys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _groupBys).GetEnumerator();
        }

        private class GroupBySelectableComparer : IEqualityComparer<IDbSelectable>
        {
            public bool Equals(IDbSelectable x, IDbSelectable y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                return x.Equals(y) || CheckSameColumnForGrouping(x as IDbColumn, y as IDbColumn);
            }

            /// <summary>
            /// if column's ref and name are the same, then this two columns are the same
            /// for groupping. this will make columns that have different alias name treated as the same column
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            private static bool CheckSameColumnForGrouping(IDbColumn x, IDbColumn y)
            {
                if (x == null || y == null)
                    return false;

                return x.Ref.Equals(y.Ref) && x.Name.Equals(y.Name);
            }

            public int GetHashCode(IDbSelectable obj)
            {
                return obj.GetHashCode();
            }
        }

        private static readonly IEqualityComparer<IDbSelectable> SqlSelectableComparerInstance = new GroupBySelectableComparer();
    }
}