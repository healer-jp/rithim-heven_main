using System.Collections.Generic;

public class Score
{
	public static int bpm;
	public static readonly int[][] score1 = new int[][]
	{
		new int[] {0},
		new int[] {0, 1, 0,1},
		new int[] {0,1,0,1},
		new int[] {0,1,0,1},
		new int[] {0,1,0,1},
		new int[] {0,0,0,0,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,0,0,0,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,0,0,0,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,0,0,0,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
	};
	public static readonly int[][] score2 = new int[][]
	{
		new int[] {0},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0,0,0,0,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,0,0,0,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,1,0,2},
		new int[] {0,0,3,0,0,0,4,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,1,0,2},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0, 1, 0, 1, 0, 1, 0, 1},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[] {0,0,0,1,0,0,2,0,0,0,0,0,0,0,3,0,0,0,4,0,0,0,0,0},
		new int[]{0,1,0,0,0,0,0,0}
		
	};
	public static readonly int[][] score3 = new int[][]
	{
		new int[] {1, 1, 1, 1}
	};

	public static int[][] GetScore(int bpm, int[][] score)
	{
		decimal sum = 0;
		List<int> timeScore = new List<int>();
		List<int> noteScore = new List<int>();

		for (int i = 0; i < score.Length; i++)
		{
			for (int j = 0; j < score[i].Length; j++)
			{
				decimal time = 240m / score[i].Length / bpm;

				if (score[i][j] > 0)
				{
					timeScore.Add((int)(sum * 1000));
					noteScore.Add(score[i][j]);
				}

				sum += time;
			}
		}

		return new int[][] { timeScore.ToArray(), noteScore.ToArray() };
	}
}
