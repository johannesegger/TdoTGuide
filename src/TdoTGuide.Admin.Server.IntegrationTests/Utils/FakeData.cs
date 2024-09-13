﻿using Bogus;
using Microsoft.Graph.Models.CallRecords;
using TdoTGuide.Admin.Shared;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests.Utils;

public static class FakeData
{
    public static Faker<Project> ProjectFaker { get; } = new Faker<Project>()
        .CustomInstantiator(v =>
        {
            var timeSelection = v.Random.Number(1, 3) switch
            {
                1 => (ITimeSelection)new ContinuousTimeSelection(),
                2 => new RegularTimeSelection(v.Random.Number(1, 24) * 5),
                _ => new IndividualTimeSelection(
                    Enumerable.Range(1, v.Random.Number(1, 10))
                        .Select(_ =>
                        {
                            var dayOffset = v.Random.Number(1, 365);
                            var hourOffset = v.Random.Number(8, 17);
                            var minuteOffset = v.Random.Number(0, 3) * 15;
                            return DateTime.Today.AddDays(dayOffset).AddHours(hourOffset).AddMinutes(minuteOffset);
                        })
                        .ToList()
                )
            };
            var date = v.Date.SoonDateOnly(20);
            var organizers = ProjectOrganizers!
                .OrderBy(_ => v.Random.Double())
                .Take(v.Random.Number(1, 5))
                .ToList();
            return new Project(
                Id: v.Random.Uuid().ToString(),
                Title: v.Random.Words(),
                Description: v.Lorem.Sentences(),
                Group: v.Random.Word(),
                Departments: v.Random.ArrayElements([.. Enumerable.Range(1, 10).Select(v => $"{v}")]),
                Building: v.Random.Int(1, 3).ToString(),
                Location: v.Address.BuildingNumber(),
                Organizer: organizers.First(),
                CoOrganizers: organizers.Skip(1).ToList(),
                TimeSelection: timeSelection
            );
        });

    public static Faker<EditingProjectDataDto> EditingProjectDataDtoFaker { get; } = new Faker<EditingProjectDataDto>()
        .CustomInstantiator(v =>
        {
            var date = v.Date.SoonDateOnly(20);
            var organizerIds = ProjectOrganizers!
                .OrderBy(_ => v.Random.Double())
                .Take(v.Random.Number(1, 5))
                .Select(v => v.Id)
                .ToList();
            var timeType = v.Random.Enum<TimeSelectionTypeDto>();
            var regularTimeSelectionInterval =
                timeType == TimeSelectionTypeDto.Regular
                ? v.Random.Number(1, 24) * 5
                : v.Random.Number(-10, 10) * 5;
            var individualTimes =
                timeType == TimeSelectionTypeDto.Individual
                ? Enumerable.Range(1, v.Random.Number(1, 10))
                    .Select(_ =>
                    {
                        var dayOffset = v.Random.Number(1, 365);
                        var hourOffset = v.Random.Number(8, 17);
                        var minuteOffset = v.Random.Number(0, 3) * 15;
                        return DateTime.Today.AddDays(dayOffset).AddHours(hourOffset).AddMinutes(minuteOffset);
                    })
                    .ToList()
                : Enumerable.Range(1, v.Random.Number(0, 10))
                    .Select(_ =>
                    {
                        var dayOffset = v.Random.Number(-365, 365);
                        var hourOffset = v.Random.Number(8, 17);
                        var minuteOffset = v.Random.Number(0, 3) * 15;
                        return DateTime.Today.AddDays(dayOffset).AddHours(hourOffset).AddMinutes(minuteOffset);
                    })
                    .ToList();
            var timeSelection = new TimeSelectionDto(timeType, regularTimeSelectionInterval, individualTimes);
            return new EditingProjectDataDto(
                Title: v.Random.Words(),
                Description: v.Lorem.Sentences(),
                Group: v.Random.Word(),
                Departments: v.Random.ArrayElements([.. Enumerable.Range(0, 10).Select(v => $"{v}")]),
                MediaFileNames: v.Random.WordsArray(0, 5),
                MediaFileNamesToRemove: v.Random.WordsArray(0, 5),
                Building: v.Random.Int(1, 3).ToString(),
                Location: v.Address.BuildingNumber(),
                OrganizerId: organizerIds.First(),
                CoOrganizerIds: organizerIds.Skip(1).ToList(),
                TimeSelection: timeSelection
            );
        });

    public static IReadOnlyList<ProjectOrganizer> ProjectOrganizers { get; } =
        new Faker<ProjectOrganizer>()
            .CustomInstantiator(v => new ProjectOrganizer(
                v.Random.Uuid().ToString(),
                v.Name.FirstName(),
                v.Name.LastName(),
                v.Random.String2(4).ToUpper()
            ))
        .Generate(10);
}
