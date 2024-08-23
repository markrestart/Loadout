using System.Collections.Generic;
using UnityEngine;

public static class Shuffler{
    public static List<T> Shuffle<T>(List<T> list){
        List<T> shuffledList = new List<T>(list);
        for(int i = 0; i < shuffledList.Count; i++){
            int randomIndex = Random.Range(0, shuffledList.Count);
            T temp = shuffledList[i];
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }
        return shuffledList;
    }
}