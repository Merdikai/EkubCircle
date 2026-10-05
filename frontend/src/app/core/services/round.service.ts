import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Round, PayoutRoundResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class RoundService {
  private http = inject(HttpClient);

  getRounds(circleId: number): Observable<Round[]> {
    return this.http.get<Round[]>(`/api/circles/${circleId}/rounds`);
  }

  getCurrentRound(circleId: number): Observable<Round | null> {
    return this.http.get<Round | null>(`/api/circles/${circleId}/rounds/current`);
  }

  payoutRound(roundId: number): Observable<PayoutRoundResponse> {
    return this.http.post<PayoutRoundResponse>(`/api/rounds/${roundId}/payout`, {});
  }
}
