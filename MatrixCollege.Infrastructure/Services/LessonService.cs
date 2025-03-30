﻿using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;

namespace Matrix;

public class LessonService : ILessonService
{
    // DI's
    private readonly MatrixCollegeContext _db;
    private readonly IProgressService _progressService;
    private readonly IValidationService _validationService;
    private readonly ILessonDao _lessonDao;
    private readonly IMapper _mapper;

    // Constructor
    public LessonService(MatrixCollegeContext db, IMapper mapper, IProgressService progressService,
                        ILessonDao lessonDao, IValidationService validationService)
    {
        _db = db;
        _progressService = progressService;
        _mapper = mapper;
        _lessonDao = lessonDao;
        _validationService = validationService;
    }

    // Methods
    public async Task<List<LessonDto>> GetAllLessonsDtoAsync()
    {
        List<LessonDto> dtoLessons = new List<LessonDto>();

        // Map to DTO objects
        List<Lesson> dbLessons = await _lessonDao.GetAllLessonsAsync();
        dbLessons.ForEach(lesson => dtoLessons.Add(_mapper.Map<LessonDto>(lesson)));

        return dtoLessons;
    }

    public async Task<LessonDto?> GetLessonDtoByIdAsync(Guid id)
    {
        Lesson? lesson = await _lessonDao.GetLessonByIdAsync(id);

        if (lesson == null) return null;
        
        return _mapper.Map<LessonDto>(lesson);
    }

    public async Task<List<LessonDto>?> AddLessonsAsync(List<LessonDto> lessonDtos)
    {
        // Fail for empty lists
        if (lessonDtos.Count == 0) return null;

        // Convert to Lesson objects
        List<Lesson> lessons = new List<Lesson>();

        foreach (LessonDto lessonDto in lessonDtos)
        {
            lessons.Add(_mapper.Map<Lesson>(lessonDto)); // Save to actual lessons list
        }

        // Verify all lessons have valid courseIds
        if (! await _lessonDao.IsLessonsValidCourses(lessons))
            return null;

        await _lessonDao.AddLessonsAsync(lessons);

        // Map to DTO
        List<LessonDto> dbLessonDtos = lessons.Select(lesson => _mapper.Map<LessonDto>(lesson)).ToList();

        return dbLessonDtos;
    }

    public async Task<List<LessonDto>?> GetLessonsDtoByCourseIdAsync (Guid courseId)
    {
        if (!(await _validationService.IsCourseExistsAsync(courseId)))
            return null;

        List<LessonDto> dtoLessons = new List<LessonDto>();

        List<Lesson> dbLessons = await _lessonDao.GetLessonsByCourseIdAsync(courseId);
        dbLessons.ForEach(lesson => dtoLessons.Add(_mapper.Map<LessonDto>(lesson)));

        return dtoLessons;
    }

    public async Task<List<Lesson>> GetLessonsByCourseIdAsync(Guid courseId)
    {
        return await _lessonDao.GetLessonsByCourseIdAsync(courseId);
    }

    public async Task<List<LessonInfoDto>?> GetLessonsInfoByCourseIdAsync(Guid courseId)
    {
        if (!(await _validationService.IsCourseExistsAsync(courseId)))
            return null;

        List<LessonInfoDto> dtoLessons = new List<LessonInfoDto>();

        List<Lesson> dbLessons = await _lessonDao.GetLessonsByCourseIdAsync(courseId);
        dbLessons.ForEach(lesson => dtoLessons.Add(_mapper.Map<LessonInfoDto>(lesson)));

        return dtoLessons;
    }

    public async Task<bool> RemoveLessonsAsync(List<Guid> lessonIds)
    {
        List<Lesson> lessons = await _lessonDao.GetLessonsByList(lessonIds);

        if (lessons.Count == 0)
            return false;

        await _lessonDao.RemoveLessonsAsync(lessons);

        return true;
    }

    public async Task<bool> RemoveLessonsByCourseId(Guid courseId)
    {
        List<Lesson> lessons = await _lessonDao.GetLessonsByCourseIdAsync(courseId);

        if (lessons.Count == 0)
            return false;
        // Remove related progresses
        await _progressService.RemoveProgressByLessonsAsync(lessons);

        await _lessonDao.RemoveLessonsAsync(lessons);

        return true;
    }

    public async Task<List<LessonDto>?> UpdateLessonsAsync(List<LessonDto> lessonDtos)
    {
        if (lessonDtos.Count == 0) return null;

        // Map to Lesson objects
        List<Lesson> lessons = new List<Lesson>();

        foreach (LessonDto lessonDto in lessonDtos)
        {
            if (!(await _validationService.IsLessonExistsAsync(lessonDto.Id)))
                return null; // Break and return null if one of the lessons does not exist

            lessons.Add(_mapper.Map<Lesson>(lessonDto));
        }

        await _lessonDao.UpdateLessonsAsync(lessons);

        // Map to Dto
        List<LessonDto> result = new List<LessonDto>();

        foreach (Lesson lesson in lessons)
            result.Add(_mapper.Map<LessonDto>(lesson));

        return result;
    }
}
