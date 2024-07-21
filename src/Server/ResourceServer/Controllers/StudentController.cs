using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ResourceServer.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public sealed class StudentController : ControllerBase
{
	private static HashSet<Student> DATA = new([
					new Student(Guid.NewGuid().ToString(),"张三"),
					new Student(Guid.NewGuid().ToString(),"李四"),
					new Student(Guid.NewGuid().ToString(),"王五"),
	], StudentComparer.Shread);

	[HttpGet]
	public IActionResult Hello()
	{
		return Ok("你好 这里是学生管理系统,如果需要访问数据请先登录");
	}


	[HttpGet]
	[Authorize]
	public async Task<IActionResult> GetStudents()
	{
		await Task.Delay(100);
		return Ok(DATA.ToList());
	}


	public record CreateStudentRequest(string Name);
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> CreateStudent(CreateStudentRequest request)
	{
		await Task.Delay(100);
		var stu = new Student(Guid.NewGuid().ToString(), request.Name);
		DATA.Add(stu);
		return Ok(stu);
	}

	[HttpGet]
	[Authorize]
	public async Task<IActionResult> DeleteStudent(string id)
	{

		await Task.Delay(100);
		var stu = DATA.FirstOrDefault(x => x.Id == id);
		if (stu is null)
		{
			return NotFound();
		}
		DATA.Remove(stu);

		return Ok(stu);
	}


}

public record Student(string Id, string name);
public sealed class StudentComparer : IEqualityComparer<Student>
{
	public static StudentComparer Shread = new();
	public bool Equals(Student? x, Student? y)
	{
		if (x == null || y == null)
		{
			return false;
		}

		return x.Id == y.Id;
	}

	public int GetHashCode([DisallowNull] Student obj)
	{
		return obj.GetHashCode() * 41;
	}
}
