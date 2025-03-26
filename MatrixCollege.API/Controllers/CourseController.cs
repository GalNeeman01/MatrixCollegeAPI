﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matrix;

[Route("/api/v1/[Controller]")]
[ApiController]
public class CoursesController : ControllerBase
{
    // DI's
    private ICourseService _courseService;

    // Constructor
    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // Routes
    [Authorize(Roles = "Professor")]
    [HttpPost]
    public async Task<IActionResult> CreateCourseAsync([FromBody] CourseDto courseDto)
    {
        // Retreive created courseDto
        CourseDto createdCourse = await _courseService.CreateCourseAsync(courseDto);

        return Created("/", createdCourse);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCoursesAsync()
    {
        List<CourseDto> courses = await _courseService.GetAllCoursesDtoAsync();

        return Ok(courses);
    }

    [HttpGet("{courseId}")]
    public async Task<IActionResult> GetCourseByIdAsync([FromRoute] Guid courseId)
    {
        CourseDto? course = await _courseService.GetCourseDtoByIdAsync(courseId);

        if (course == null)
            return NotFound(new ResourceNotFoundError(courseId.ToString()));

        return Ok(course);
    }

    [HttpGet("lesson/{lessonId}")]
    public async Task<IActionResult> GetCourseByLessonIdAsync([FromRoute] Guid lessonId)
    {
        CourseDto? course = await _courseService.GetCourseDtoByLessonIdAsync(lessonId);

        if (course == null)
            return NotFound(new ResourceNotFoundError(lessonId.ToString()));

        return Ok(course);
    }

    [Authorize(Roles = "Professor")]
    [HttpDelete("{courseId}")]
    public async Task<IActionResult> RemoveCourseAsync([FromRoute] Guid courseId)
    {
        bool result = await _courseService.RemoveCourseAsync(courseId);

        if (!result)
            return NotFound(new ResourceNotFoundError(courseId.ToString()));

        return NoContent();
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    public async Task<IActionResult> UpdateCourseAsync([FromBody] CourseDto courseDto)
    {
        CourseDto? result = await _courseService.UpdateCourseAsync(courseDto);

        if (result == null)
            return NotFound(new ResourceNotFoundError(courseDto.Id.ToString()));

        return NoContent();
    }
}
