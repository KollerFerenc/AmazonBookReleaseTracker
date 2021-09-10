using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class TrackingDataAnalyzer
    {
        private readonly TrackingData _oldData;
        private readonly TrackingData _newData;

        public TrackingDataAnalyzer(TrackingData oldData, TrackingData newData)
        {
            _oldData = oldData;
            _newData = newData;
        }

        public Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>> GetAll()
        {
            if (_oldData.IsEmpty && _newData.IsEmpty)
            {
                return new Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>>(
                    Array.Empty<AmazonSeries>(), Array.Empty<AmazonBook>());
            }
            else if (_newData.IsEmpty)
            {
                return new Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>>(
                    _oldData.AmazonSeries, _oldData.AmazonBooks);
            }
            else if (_oldData.IsEmpty)
            {
                return new Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>>(
                    _newData.AmazonSeries, _newData.AmazonBooks);
            }

            return new Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>>(
                    _newData.AmazonSeries, _newData.AmazonBooks);
        }

        public Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>> GetNew()
        {
            if (_oldData.IsEmpty && _newData.IsEmpty)
            {
                return new Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>>(
                    Array.Empty<AmazonSeries>(), Array.Empty<AmazonBook>());
            }
            else if (_newData.IsEmpty)
            {
                return new Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>>(
                    _oldData.AmazonSeries, _oldData.AmazonBooks);
            }
            else if (_oldData.IsEmpty)
            {
                return new Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>>(
                    _newData.AmazonSeries, _newData.AmazonBooks);
            }

            var amazonSeries = new List<AmazonSeries>();
            var amazonBooks = new List<AmazonBook>();

            foreach (var series in _newData.AmazonSeries)
            {
                var tempSeries = new AmazonSeries(series.AmazonId)
                {
                    Title = series.Title
                };

                if (_oldData.AmazonSeries.Contains(tempSeries))
                {
                    int index = _oldData.AmazonSeries.IndexOf(tempSeries);
                    var old = _oldData.AmazonSeries[index];

                    tempSeries.Books.AddRange(series.Books.Except(old.Books));

                    if (tempSeries.Books.Count > 0)
                    {
                        amazonSeries.Add(tempSeries);
                    }
                }
                else
                {
                    amazonSeries.Add(tempSeries);
                }
            }

            amazonBooks.AddRange(_newData.AmazonBooks.Except(_oldData.AmazonBooks));

            return new Tuple<IEnumerable<AmazonSeries>, IEnumerable<AmazonBook>>(
                    amazonSeries, amazonBooks);
        }
    }
}
