import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Payment, RecordPaymentRequest, PaymentHistoryFilter } from '../models';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);

  recordPayment(request: RecordPaymentRequest): Observable<Payment> {
    return this.http.post<Payment>(`/api/rounds/${request.roundId}/payments`, request);
  }

  getPayments(filter?: PaymentHistoryFilter): Observable<Payment[]> {
    if (filter?.circleId) {
      return this.http.get<Payment[]>(`/api/circles/${filter.circleId}/history`);
    }
    return this.http.get<Payment[]>('/api/history');
  }
}
