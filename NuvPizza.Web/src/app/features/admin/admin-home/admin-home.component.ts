import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-home',
  standalone: true,
  templateUrl: './admin-home.html',
  styleUrls: ['./admin-home.css']
})
export class AdminHomeComponent {
  constructor(private router: Router) {}

  navegarPara(destino: 'pedidos' | 'produtos') {
    if (destino === 'pedidos') {
      this.router.navigate(['/admin/painel']);
    } else {
      this.router.navigate(['/admin/produtos']); // Vamos criar essa rota já já
    }
  }
}