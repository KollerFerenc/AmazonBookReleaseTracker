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
            _oldData = oldData ?? new();
            _newData = newData ?? new();
        }

        public AmazonContainer GetAll()
        {
            if (_oldData.IsEmpty && _newData.IsEmpty)
            {
                return new AmazonContainer();
            }
            else if (_newData.IsEmpty)
            {
                return new AmazonContainer(_oldData.AmazonSeries, _oldData.AmazonBooks);
            }
            else if (_oldData.IsEmpty)
            {
                return new AmazonContainer(_newData.AmazonSeries, _newData.AmazonBooks);
            }

            return new AmazonContainer(_newData.AmazonSeries, _newData.AmazonBooks);
        }

        public AmazonContainer GetNew()
        {
            if (_oldData.IsEmpty && _newData.IsEmpty)
            {
                return new AmazonContainer();
            }
            else if (_newData.IsEmpty)
            {
                return new AmazonContainer(_oldData.AmazonSeries, _oldData.AmazonBooks);
            }
            else if (_oldData.IsEmpty)
            {
                return new AmazonContainer(_newData.AmazonSeries, _newData.AmazonBooks);
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

            return new AmazonContainer(amazonSeries, amazonBooks);
        }
    }
}
