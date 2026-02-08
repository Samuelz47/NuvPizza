import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PainelPedidosComponent } from './painel-pedidos.component';

describe('PainelPedidosComponent', () => {
  let component: PainelPedidosComponent;
  let fixture: ComponentFixture<PainelPedidosComponent>;  
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PainelPedidosComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PainelPedidosComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});