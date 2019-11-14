using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class WeightedRandomBag<T>  {

    private struct Entry {
        public double accumulatedWeight;
        public T item;
    }

    private List<Entry> entries = new List<Entry>();
    private double accumulatedWeight;
    private Random rand = new Random();

    public void AddEntry(T item, double weight) {
        accumulatedWeight += weight;
        entries.Add(new Entry { item = item, accumulatedWeight = accumulatedWeight });
    }

    public T GetRandom() {
        double r = rand.NextDouble() * accumulatedWeight;

        foreach (Entry entry in entries) {
            if (entry.accumulatedWeight >= r) {
                return entry.item;
            }
        }
        return default(T);
    }
}