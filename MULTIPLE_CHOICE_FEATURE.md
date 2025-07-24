# Multiple Choice Hiragana Questions

This document describes the implementation of multiple choice questions for hiragana learning.

## Overview

The bot now supports multiple choice questions for hiragana characters, providing users with 4 options to choose from instead of requiring them to type the answer manually.

## Features

- **Multiple Choice Format**: Each question presents 4 options:
  - 1 correct answer (the actual romaji)
  - 1 similar wrong answer (shares first character or similar sounds)
  - 2 dissimilar wrong answers (random selections)

- **Interactive Interface**: Uses inline keyboard buttons for answer selection

- **Same Scoring System**: Maintains the existing streak system and attempt limits (3 attempts max)

## Usage

### Commands

1. **Regular Text Questions**: `/test` - Traditional text input questions
2. **Multiple Choice Questions**: `/test mc` - New multiple choice format
3. **Dedicated Command**: `/multiplechoice` - Direct access to multiple choice questions

### Example Flow

1. User sends `/test mc`
2. Bot displays hiragana character image with 4 button options
3. User clicks their choice
4. Bot provides immediate feedback with streak information

## Technical Implementation

### Database Changes

- Added `MultipleChoiceOptions` field to Question entity (JSON string)
- Added `QuestionType.MultipleChoiceHiragana` enum value

### New Components

- `MultipleChoiceAnswerCallbackHandler` - Processes button clicks
- `MultipleChoiceAnswerCallbackData` - Callback data model
- Enhanced `HiraganaService` with multiple choice methods
- Smart wrong answer generation with similarity algorithms

### Answer Selection Algorithm

The system generates wrong answers using:

1. **Similar answers**: Characters with same starting sound or phonetic similarity
2. **Dissimilar answers**: Random selection from remaining characters
3. **Fallback logic**: Ensures 4 options even with limited data

## Database Migration

A migration file has been created to add the `MultipleChoiceOptions` field:
- File: `20250724000000_AddMultipleChoiceOptions.cs`
- Run `dotnet ef database update` to apply when building the project

## Testing

Comprehensive tests cover:
- Multiple choice question generation
- Answer processing (correct/incorrect)
- Attempt counting and question expiration
- Wrong answer generation algorithms

## Future Enhancements

- Support for katakana multiple choice
- Configurable number of options
- Difficulty-based similar answer selection
- Analytics on option selection patterns