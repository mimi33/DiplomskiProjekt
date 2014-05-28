using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DiplomskiProjekt.Classes
{
    public class Populacija : IList<Jedinka>
    {
        readonly List<Jedinka> _populacija;

        private int _indexNajboljeJedinke;
        //private Jedinka _najboljaJedinka;
        public Jedinka NajboljaJedinka
        {
            get
            {
                if (_populacija.Count == 0)
                    return null;
                if (_indexNajboljeJedinke == -1)
                {
                    var min = _populacija.Min(x => x.GreskaJedinke);
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    _indexNajboljeJedinke = _populacija.IndexOf(_populacija.First(x => x.GreskaJedinke == min));
                }
                return _populacija[_indexNajboljeJedinke];
            }
        }

        public Populacija()
        {
            _populacija = new List<Jedinka>();
            _indexNajboljeJedinke = -1;
        }

        /// <summary>
        /// Generira novu random populaciju
        /// </summary>
        public Populacija(int velicinaPopulacije)
        {
            _indexNajboljeJedinke = -1;
            _populacija = new List<Jedinka>();
            for (var i = 0; i < velicinaPopulacije; i++)
            {
                var j = new Jedinka();
                j.FiksirajKonstante();
                GP.EvaluationOp.IzracunajGresku(j);
                Add(j);
            }
        }

        //public bool ProvijeriPreuzimanje() // todo ovo ne bi smijelo trebati
        //{
        //    // ReSharper disable once CompareOfFloatsByEqualityOperator
        //    var najgoraJedinka = _populacija.First(j => j.GreskaJedinke == _populacija.Min(x => x.GreskaJedinke));
        //    return
        //        (string.Compare(NajboljaJedinka.ToString(), najgoraJedinka.ToString(), StringComparison.InvariantCulture) == 0);
        //}

        public Jedinka IzracunajNajboljuJedinku()
        {
            _indexNajboljeJedinke = -1;
            return NajboljaJedinka;
        }

        #region IList wrapper

        public int IndexOf(Jedinka item)
        {
            return _populacija.IndexOf(item);
        }

        public void Insert(int index, Jedinka item)
        {
            _populacija.Insert(index, item);
            if (_indexNajboljeJedinke == index)
                _indexNajboljeJedinke = -1;
        }

        public void RemoveAt(int index)
        {
            _populacija.RemoveAt(index);
            if (index == _indexNajboljeJedinke)
            //if (NajboljaJedinka == _populacija[index])
                _indexNajboljeJedinke = -1;
        }

        public Jedinka this[int index]
        {
            get { return _populacija[index]; }
            set
            {
                if (_indexNajboljeJedinke == index)
                    _indexNajboljeJedinke = -1;
                _populacija[index] = value;
            }
        }

        public void Add(Jedinka jedinka)
        {
            if (double.IsNaN(jedinka.GreskaJedinke))
                throw new Exception();
            _populacija.Add(jedinka);
            if (_indexNajboljeJedinke == -1 || jedinka.GreskaJedinke < _populacija[_indexNajboljeJedinke].GreskaJedinke)
                _indexNajboljeJedinke = Count - 1;
        }

        public void Clear()
        {
            _populacija.Clear();
            _indexNajboljeJedinke = -1;
        }

        public bool Contains(Jedinka jedinka)
        {
            return _populacija.Contains(jedinka);
        }

        public void CopyTo(Jedinka[] array, int arrayIndex)
        {
            _populacija.CopyTo(array, arrayIndex);
        }

        public bool Remove(Jedinka item)
        {
            if (_indexNajboljeJedinke == IndexOf(item))
                _indexNajboljeJedinke = -1;
            return _populacija.Remove(item);
        }

        public int Count
        {
            get { return _populacija.Count; } 
        }

        public bool IsReadOnly
        {
            get {return false;}
        }

        public IEnumerator<Jedinka> GetEnumerator()
        {
            return _populacija.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
    