using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Nuke.Common.ProjectModel;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace helpers
{

	[SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
	[SuppressMessage("Usage", "S3267:Loop should be simplified by calling Select", 
		Justification = "Not production code.")]
	public static class GetSolutionAllProjects
	{
		public static List<string> GetSolutionAllProjectsList(Solution solution)
		{
			if ( solution.Directory == null )
			{
				throw new ArgumentNullException(nameof(Solution));
			}
			
			var slnListOutput =  
				DotNet($"sln {solution.ToString()} list", null, 
					null, null, false);

			var result = new List<string>();
			foreach ( var slnListOutputItem in slnListOutput )
			{
				if ( slnListOutputItem.Text.Contains("---") || 
				     slnListOutputItem.Text.Contains("Project") || 
				     slnListOutputItem.Text.Contains("_build"))
				{
					continue;
				}
				
				var fullPath = Path.Combine(solution.Directory, slnListOutputItem.Text);
				
				if ( File.Exists(fullPath) )
				{
					result.Add(fullPath);
				}
			}
			return result;
		}
	}
	
}
