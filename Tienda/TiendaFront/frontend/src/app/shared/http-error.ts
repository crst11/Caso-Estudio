export function getErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error !== null && 'error' in error) {
    const detail = (error as { error?: unknown }).error;

    if (typeof detail === 'string' && detail.trim()) {
      return detail.trim();
    }
  }

  return fallback;
}
