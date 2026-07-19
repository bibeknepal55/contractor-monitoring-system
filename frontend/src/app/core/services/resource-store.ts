import { BehaviorSubject, Observable } from 'rxjs';

export interface ResourceState<T> {
  data: T[];
  total: number;
  loading: boolean;
  error: string | null;
  loaded: boolean;
}

export class ResourceStore<T> {
  private state: ResourceState<T> = {
    data: [],
    total: 0,
    loading: false,
    error: null,
    loaded: false,
  };

  private stateSubject = new BehaviorSubject<ResourceState<T>>(this.state);
  public state$: Observable<ResourceState<T>> = this.stateSubject.asObservable();

  get current(): ResourceState<T> {
    return this.state;
  }

  setLoading(loading: boolean): void {
    this.update({ loading, error: loading ? null : this.state.error });
  }

  setError(error: string): void {
    this.update({ error, loading: false });
  }

  setData(data: T[], total: number): void {
    this.update({ data, total, loading: false, error: null, loaded: true });
  }

  clear(): void {
    this.state = { data: [], total: 0, loading: false, error: null, loaded: false };
    this.stateSubject.next(this.state);
  }

  private update(patch: Partial<ResourceState<T>>): void {
    this.state = { ...this.state, ...patch };
    this.stateSubject.next(this.state);
  }
}