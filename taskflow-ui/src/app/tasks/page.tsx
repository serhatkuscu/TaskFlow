'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { authService } from '@/services/authService';
import { taskService } from '@/services/taskService';
import type { TaskItem, TaskStatus } from '@/types';

interface ModalState {
  open: boolean;
  mode: 'create' | 'edit';
  task?: TaskItem;
}

const STATUS_LABEL: Record<TaskStatus, string> = {
  Pending:    'Pending',
  InProgress: 'In Progress',
  Completed:  'Completed',
  Cancelled:  'Cancelled',
};

const STATUS_STYLE: Record<TaskStatus, string> = {
  Pending:    'bg-yellow-100 text-yellow-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Completed:  'bg-green-100 text-green-800',
  Cancelled:  'bg-gray-100 text-gray-500',
};

type Transition = { label: string; next: TaskStatus };

function transitions(status: TaskStatus): Transition[] {
  if (status === 'Pending')    return [{ label: 'Start', next: 'InProgress' }, { label: 'Cancel', next: 'Cancelled' }];
  if (status === 'InProgress') return [{ label: 'Complete', next: 'Completed' }, { label: 'Cancel', next: 'Cancelled' }];
  return [];
}

export default function TasksPage() {
  const router = useRouter();

  const [ready, setReady]       = useState(false);
  const [tasks, setTasks]       = useState<TaskItem[]>([]);
  const [loading, setLoading]   = useState(true);
  const [error, setError]       = useState('');

  const [modal, setModal]             = useState<ModalState>({ open: false, mode: 'create' });
  const [formTitle, setFormTitle]     = useState('');
  const [formDesc, setFormDesc]       = useState('');
  const [formError, setFormError]     = useState('');
  const [formLoading, setFormLoading] = useState(false);

  // Protected route
  useEffect(() => {
    if (!authService.isAuthenticated()) {
      router.replace('/login');
    } else {
      setReady(true);
    }
  }, [router]);

  const fetchTasks = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await taskService.getAll();
      setTasks(data.items);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load tasks.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (ready) fetchTasks();
  }, [ready, fetchTasks]);

  function openCreate() {
    setFormTitle('');
    setFormDesc('');
    setFormError('');
    setModal({ open: true, mode: 'create' });
  }

  function openEdit(task: TaskItem) {
    setFormTitle(task.title);
    setFormDesc(task.description ?? '');
    setFormError('');
    setModal({ open: true, mode: 'edit', task });
  }

  function closeModal() {
    setModal({ open: false, mode: 'create' });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const title = formTitle.trim();
    if (title.length < 3) {
      setFormError('Title must be at least 3 characters.');
      return;
    }
    setFormError('');
    setFormLoading(true);
    try {
      const desc = formDesc.trim() || undefined;
      if (modal.mode === 'create') {
        await taskService.create(title, desc);
      } else if (modal.task) {
        await taskService.update(modal.task.id, title, desc);
      }
      closeModal();
      fetchTasks();
    } catch (err: unknown) {
      setFormError(err instanceof Error ? err.message : 'Save failed.');
    } finally {
      setFormLoading(false);
    }
  }

  async function handleDelete(id: string) {
    if (!confirm('Delete this task?')) return;
    try {
      await taskService.delete(id);
      fetchTasks();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Delete failed.');
    }
  }

  async function handleStatus(task: TaskItem, next: TaskStatus) {
    try {
      await taskService.updateStatus(task.id, next);
      fetchTasks();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Status update failed.');
    }
  }

  function handleLogout() {
    authService.logout();
    router.push('/login');
  }

  if (!ready) return null;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900">TaskFlow</h1>
        <div className="flex items-center gap-3">
          <button
            onClick={openCreate}
            className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
          >
            + New Task
          </button>
          <button
            onClick={handleLogout}
            className="px-4 py-2 text-sm text-gray-500 hover:text-gray-900 transition-colors"
          >
            Logout
          </button>
        </div>
      </header>

      {/* Body */}
      <main className="max-w-3xl mx-auto px-6 py-8">
        {error && (
          <div className="mb-4 px-4 py-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm flex justify-between">
            <span>{error}</span>
            <button onClick={() => setError('')} className="ml-4 text-red-400 hover:text-red-600">✕</button>
          </div>
        )}

        {loading ? (
          <p className="text-center py-20 text-gray-400">Loading tasks…</p>
        ) : tasks.length === 0 ? (
          <div className="text-center py-20 text-gray-400">
            <p className="text-lg mb-2">No tasks yet.</p>
            <button onClick={openCreate} className="text-blue-600 hover:underline text-sm">
              Create your first task
            </button>
          </div>
        ) : (
          <div className="space-y-3">
            {tasks.map((task) => (
              <div
                key={task.id}
                className="bg-white border border-gray-200 rounded-lg p-4 flex items-start justify-between gap-4"
              >
                {/* Left: info */}
                <div className="flex-1 min-w-0">
                  <div className="mb-1">
                    <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_STYLE[task.status]}`}>
                      {STATUS_LABEL[task.status]}
                    </span>
                  </div>
                  <h3 className="font-medium text-gray-900">{task.title}</h3>
                  {task.description && (
                    <p className="text-sm text-gray-500 mt-0.5 line-clamp-2">{task.description}</p>
                  )}
                  <p className="text-xs text-gray-400 mt-1">
                    {new Date(task.createdAt).toLocaleDateString()}
                  </p>
                </div>

                {/* Right: actions */}
                <div className="flex flex-col gap-2 shrink-0">
                  {transitions(task.status).length > 0 && (
                    <div className="flex gap-1">
                      {transitions(task.status).map(({ label, next }) => (
                        <button
                          key={next}
                          onClick={() => handleStatus(task, next)}
                          className="px-2 py-1 text-xs rounded border border-gray-300 text-gray-600 hover:bg-gray-50 transition-colors"
                        >
                          {label}
                        </button>
                      ))}
                    </div>
                  )}
                  <div className="flex gap-1">
                    <button
                      onClick={() => openEdit(task)}
                      className="px-2 py-1 text-xs rounded border border-gray-300 text-gray-600 hover:bg-gray-50 transition-colors"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => handleDelete(task.id)}
                      className="px-2 py-1 text-xs rounded border border-red-200 text-red-600 hover:bg-red-50 transition-colors"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </main>

      {/* Modal */}
      {modal.open && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">
              {modal.mode === 'create' ? 'New Task' : 'Edit Task'}
            </h2>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Title</label>
                <input
                  type="text"
                  value={formTitle}
                  onChange={(e) => setFormTitle(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Task title (min 3 chars)"
                  autoFocus
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Description <span className="text-gray-400 font-normal">(optional)</span>
                </label>
                <textarea
                  value={formDesc}
                  onChange={(e) => setFormDesc(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
                  placeholder="Add a description…"
                  rows={3}
                />
              </div>

              {formError && (
                <p className="text-sm text-red-600">{formError}</p>
              )}

              <div className="flex gap-2 justify-end pt-1">
                <button
                  type="button"
                  onClick={closeModal}
                  className="px-4 py-2 text-sm text-gray-600 hover:text-gray-900 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={formLoading}
                  className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
                >
                  {formLoading ? 'Saving…' : 'Save'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
