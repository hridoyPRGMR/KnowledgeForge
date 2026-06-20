import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { api } from '../api/client';

export default function QuizPage() {
  const { id, chapterId } = useParams<{ id: string; chapterId: string }>();
  const [quizType, setQuizType] = useState('mcq');
  const [currentIndex, setCurrentIndex] = useState(0);
  const [selected, setSelected] = useState<string | null>(null);
  const [score, setScore] = useState(0);
  const [showResult, setShowResult] = useState(false);
  const [finished, setFinished] = useState(false);

  const loadQuiz = useMutation({
    mutationFn: () => api.generateQuiz(id!, chapterId!, quizType),
    onSuccess: () => { setCurrentIndex(0); setScore(0); setFinished(false); setShowResult(false); setSelected(null); },
  });

  const quiz = loadQuiz.data;
  const question = quiz?.questions[currentIndex];

  const checkAnswer = () => {
    if (!question || !selected) return;
    setShowResult(true);
    if (selected === question.correctAnswer) setScore((s) => s + 1);
  };

  const next = () => {
    if (!quiz) return;
    if (currentIndex + 1 >= quiz.questions.length) {
      setFinished(true);
    } else {
      setCurrentIndex((i) => i + 1);
      setSelected(null);
      setShowResult(false);
    }
  };

  return (
    <div>
      <Link to={`/books/${id}`} className="back-link">← Back to Book</Link>
      <h1>Quiz</h1>

      <div className="quiz-controls">
        <select value={quizType} onChange={(e) => setQuizType(e.target.value)}>
          <option value="mcq">Multiple Choice</option>
          <option value="flashcard">Flashcards</option>
          <option value="truefalse">True / False</option>
        </select>
        <button className="btn primary" onClick={() => loadQuiz.mutate()} disabled={loadQuiz.isPending}>
          {loadQuiz.isPending ? 'Generating...' : quiz ? 'Regenerate' : 'Generate Quiz'}
        </button>
      </div>

      {finished && (
        <div className="quiz-result">
          <h2>Quiz Complete!</h2>
          <p>Score: {score} / {quiz?.questions.length}</p>
        </div>
      )}

      {question && !finished && (
        <div className="quiz-card">
          <p className="quiz-progress">Question {currentIndex + 1} of {quiz!.questions.length}</p>
          <h3>{question.question}</h3>
          <div className="quiz-options">
            {(question.options.length > 0 ? question.options : [question.correctAnswer, '???']).map((opt) => (
              <button
                key={opt}
                className={`quiz-option ${selected === opt ? 'selected' : ''} ${showResult && opt === question.correctAnswer ? 'correct' : ''} ${showResult && selected === opt && opt !== question.correctAnswer ? 'wrong' : ''}`}
                onClick={() => !showResult && setSelected(opt)}
                disabled={showResult}
              >
                {opt}
              </button>
            ))}
          </div>
          {showResult && <p className="explanation">{question.explanation}</p>}
          <div className="quiz-actions">
            {!showResult ? (
              <button className="btn primary" onClick={checkAnswer} disabled={!selected}>Check</button>
            ) : (
              <button className="btn primary" onClick={next}>{currentIndex + 1 >= quiz!.questions.length ? 'Finish' : 'Next'}</button>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
