# Sql Generator

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=01ffbbe9f7191614102d37f67c1148aedb773cb9&metric=alert_status)](https://sonarcloud.io/dashboard?id=01ffbbe9f7191614102d37f67c1148aedb773cb9)
[![Build Status](https://dev.azure.com/jacksonmiller7855/Personal%20Public/_apis/build/status/Jacksondr5.sql-generator?branchName=master)](https://dev.azure.com/jacksonmiller7855/Personal%20Public/_build/latest?definitionId=10&branchName=master)

This project generates SQL scripts to perform basic CRUD operations for a simple DTO. The goal of this project is to perform 80%+ of the boilerplate work that comes along with using Dapper and stored procedures as part of an API specific to the kinds of work I find myself doing these days. It's _**NOT**_ meant to be a general purpose tool that can work in many circumstances. The SQL style conforms to the SQL style guide at my current job and I avoided writing in any features that would roughly violate [XKCD Rule 1205](https://xkcd.com/1205/).

## Limitations

- The program only considers a small list of primitive types (and `List<T>`s). For the complete list, see [this file](/Core/Utils.cs).

## Key Assumptions

- All enums have an underlying type of System.Int32
- All List<T> are stored as JSON strings. It is assumed that T is serializable
- PK columns are not updated
