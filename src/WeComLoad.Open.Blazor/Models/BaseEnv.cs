﻿namespace WeComLoad.Open.Blazor.Models
{
	public class BaseEnv<T>
	{
		public T Dev { get; set; }
		public T Test { get; set; }
		public T Prod { get; set; }
	}
}
